using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    Rigidbody RigidBody;
    private AbilityContainer MyAbility;
    GameObject CurrentRoad;
    public int LaneToUse;



    class CarRotator
    {
        Quaternion InitialRotation;
        Quaternion TargetRotation;
        float TimeToStartRotation;
        CarController Car;
        Vector3 Destination;

        public CarRotator(CarController inCar, Vector3 inDestination)
        {
            Car = inCar;
            InitialRotation = inCar.transform.rotation;
            Destination = inDestination;
            TimeToStartRotation = Time.time;
            TargetRotation = Quaternion.LookRotation(inDestination - inCar.transform.position);
        }

        public void Rotate()
        {
            float deltaTime = Time.time - TimeToStartRotation;
            const float TIME_SCALE_TO_ROTATE = 2.0f;
            Car.transform.rotation = Quaternion.Slerp(InitialRotation, TargetRotation, deltaTime * TIME_SCALE_TO_ROTATE);

            if (deltaTime * TIME_SCALE_TO_ROTATE > 1)
            {
                Car.transform.rotation = Quaternion.LookRotation(Destination - Car.transform.position);
                Car.RemoveRotator();
            }
        }
    };
    CarRotator RotatorForThisCar;
    public void RemoveRotator()
    {
        RotatorForThisCar = null;
    }

    abstract class ICarMover
    {
        protected CarController Car;
        Vector3 DestinationInLane;
        protected Vector3 DirectionOfMovement;

        public ICarMover(CarController inCar)
        {
            Car = inCar;
        }
        public void MakeDestination()
        {
            if (Car.CurrentRoad != null)
            {
            bool bIsForward = Car.IsForwardDirectionInRotatedRectangle(Car.CurrentRoad);
                DirectionOfMovement = Car.CurrentRoad.transform.forward * (bIsForward ? 1 : -1);

                var roadInfo = Car.CurrentRoad.GetComponent<RoadInfo>();
                int maxCount = bIsForward ? roadInfo.ForwardLaneCount : roadInfo.BackwardLaneCount;
                int LaneToUse = Random.Range(1, maxCount + 1) * (bIsForward ? 1 : -1);

                DestinationInLane = roadInfo.GetDestinationOfLane(LaneToUse);
            }
        }

        public void Accelerate()
        {
            if (Car.RotatorForThisCar != null)
            {
                Car.RotatorForThisCar.Rotate();
            }
            
            Car.RigidBody.AddForce(Car.transform.forward * Time.deltaTime * Car.MyAbility.GetPower(), ForceMode.Acceleration);

            Car.RigidBody.velocity = Vector3.ClampMagnitude(Car.RigidBody.velocity, Car.MyAbility.GetMaxSpeed());

        }
        public void Brake()
        {
            if (Car.RigidBody.velocity.magnitude > 3.0f)
            {
                float Power = Car.MyAbility.GetPower() * 3;
                Car.RigidBody.AddForce(-Car.RigidBody.velocity.normalized * Time.deltaTime * Power, ForceMode.Acceleration);
            }
            else
            {
                Car.RigidBody.angularVelocity = Vector3.zero;
                Car.RigidBody.velocity = Vector3.zero;
            }
        }
        public void SetDestination(Vector3 inDestination)
        {
            DestinationInLane = inDestination;
        }
        public Vector3 GetDestination()
        {
            return DestinationInLane;
        }
        public Vector3 GetDirection()
        {
            return DirectionOfMovement;
        }
        public abstract void Move();

    };

    class CarMoverAlongTheRoad : ICarMover
    {
        public CarMoverAlongTheRoad(CarController inCar) : base(inCar)
        {
            MakeDestination();
            inCar.RotatorForThisCar = new CarRotator(inCar, GetDestination());
        }

        public override void Move()
        {
            var ray = new Ray(Car.transform.position, DirectionOfMovement);
            const float SafeDistance = 15.0f;
            RaycastHit hitResult;
            Physics.Raycast(ray, out hitResult, SafeDistance);
            if ((hitResult.collider && hitResult.collider.tag == "Obstacles"))
            {
                Brake();
            }
            else
            {
                Accelerate();
            }
        }
    }
    class CarMoverUnderTheTraffic : ICarMover
    {
        bool bIsWaiting;
        public CarMoverUnderTheTraffic(CarController inCar) : base(inCar)
        {
        }

        public override void Move()
        {
            if (bIsWaiting)
            {
                Brake();
            }
            else
            {
                var remainingDistance = GetDestination() - Car.transform.position;
                if (remainingDistance.magnitude < 5)
                {
                    Car.LeaveTraffic();
                    return;
                }
                Accelerate();
            }
        }

        public void SetWait(bool bWait)
        {
            bIsWaiting = bWait;
            if(bWait)
            {
                Car.RemoveRotator();
            }
            else
            {
                Car.SetDestination(GetDestination());
            }
        }
    }

    ICarMover MoverForThisCar;

    // Start is called before the first frame update
    void Start()
    {
        RigidBody = GetComponent<Rigidbody>();
        MyAbility = GetComponent<AbilityContainer>();

        CurrentRoad = ObjectManager.Get().FindRoadOn(gameObject.transform.position);
        if(CurrentRoad == null)
        {
            Debug.Log(string.Format("error {0}", gameObject.transform.position));
        }

        MoverForThisCar = new CarMoverAlongTheRoad(this);
        RotatorForThisCar = new CarRotator(this, MoverForThisCar.GetDestination());
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (MoverForThisCar == null)
            return;

        var ray = new Ray(transform.position, MoverForThisCar.GetDirection());

        const float SafeDistance = 15.0f;
        Debug.DrawRay(transform.position, ray.direction * SafeDistance, Color.red);
        Debug.DrawLine(transform.position, MoverForThisCar.GetDestination(), Color.green);
        Debug.DrawLine(transform.position, transform.position + transform.forward * 5, Color.blue);

        MoverForThisCar.Move();

        var newRoad = ObjectManager.Get().FindRoadOn(gameObject.transform.position);
        if(newRoad && newRoad != CurrentRoad)
        {
            CurrentRoad = newRoad;
            if(MoverForThisCar as CarMoverUnderTheTraffic == null)
                MoverForThisCar = new CarMoverAlongTheRoad(this);
        }
    }
    public void SetTraffic(bool bIsSet)
    {
        if(bIsSet)
        {
            MoverForThisCar = new CarMoverUnderTheTraffic(this);
        }
        else
        {
            MoverForThisCar = new CarMoverAlongTheRoad(this);
        }
    }

    public void LeaveTraffic()
    {
        MoverForThisCar = new CarMoverAlongTheRoad(this);
        MoverForThisCar.MakeDestination();
    }
    public bool IsForwardDirectionInRotatedRectangle(GameObject road)
    {
        if (road == null) return true;

        var boxCollider = road.GetComponent<BoxCollider>();
        var center3D = road.transform.position + boxCollider.center;

        // Translate the point to the rectangle's local coordinate space
        Vector2 point = new Vector2(gameObject.transform.position.x, gameObject.transform.position.z);
        Vector2 center = new Vector2(center3D.x, center3D.z);
        Vector2 translatedPoint = point - center;

        // Apply the inverse rotation of the rectangle to the translated point
        Quaternion inverseRotation = Quaternion.Euler(0f, 0f, road.transform.rotation.eulerAngles.y);
        Vector2 rotatedPoint = inverseRotation * translatedPoint;

        return rotatedPoint.x > 0;
    }

    public void SetWait(bool bInWait)
    {
        (MoverForThisCar as CarMoverUnderTheTraffic)?.SetWait(bInWait);
    }

    public void SetDestination(Vector3 destination)
    {
        (MoverForThisCar as CarMoverUnderTheTraffic).SetDestination(destination);
        RotatorForThisCar = new CarRotator(this, destination);
    }
}
