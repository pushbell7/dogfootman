using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AbilityContainer : MonoBehaviour
{
    [System.Serializable]
    public class Ability
    {
        public float Power;
        public float Mass;
        public float MaxSpeed;
        public int Life;

        public void Add(Ability other)
        {
            Power += other.Power;
            Mass += other.Mass;
            MaxSpeed += other.MaxSpeed;
            Life += other.Life;
        }
    }
    public class DefaultAbilityFactory
    {

        class BaseFactory
        {
            public virtual Ability Make()
            {
                return new Ability();
            }
        }

        class HumanFactory : BaseFactory
        {
            public override Ability Make()
            {
                var ability = new Ability();
                ability.Power = 1000.0f;
                ability.Mass = 1.0f;
                ability.MaxSpeed = 5.0f;
                ability.Life = 3;
                return ability;
            }
        }
        class CarFactory : BaseFactory
        {
            public override Ability Make()
            {
                var ability = new Ability();
                ability.Power = 10000.0f;
                ability.Mass = 20.0f;
                ability.MaxSpeed = 10.0f;
                ability.Life = 10;
                return ability;
            }
        }

        class KickboardFactory : BaseFactory
        {
            public override Ability Make()
            {
                var ability = new Ability();
                ability.Power = 9000.0f;
                ability.Mass = 4.0f;
                ability.MaxSpeed = 4.0f;
                ability.Life = 0;
                return ability;
            }
        }

        public Ability Make(ObjectManager.ObjectType type)
        {
            if (type == ObjectManager.ObjectType.Car) { return new CarFactory().Make(); }
            else if (type == ObjectManager.ObjectType.Human) { return new HumanFactory().Make(); }
            else if(type == ObjectManager.ObjectType.MyCharacter) { return new HumanFactory().Make(); }
            else if (type == ObjectManager.ObjectType.ItemToRide) { return new KickboardFactory().Make(); }
            else { return new BaseFactory().Make(); }
        }
    }
    Ability MyAbility;

    public delegate void OnDeath(GameObject other);
    public OnDeath OnDeathDelegator;

    // Start is called before the first frame update
    void Start()
    {
        MyAbility = new DefaultAbilityFactory().Make(ObjectManager.GetType(gameObject));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetPower()
    {
        return MyAbility.Power;
    }
    public void SetPower(float newPower)
    {
        MyAbility.Power = newPower;
    }
    public float GetMaxSpeed()
    {
        return MyAbility.MaxSpeed;
    }
    public void SetMaxSpeed(float newSpeed)
    {
        MyAbility.MaxSpeed = newSpeed;
    }

    public float GetMass()
    {
        return MyAbility.Mass;
    }
    public void SetMass(float newMass)
    {
        MyAbility.Mass = newMass;
        gameObject.GetComponent<Rigidbody>().mass = newMass;
    }

    public void GetItem(AbilityContainer other)
    {
        MyAbility.Add(other.MyAbility);
    }

    public void DecreaseLife()
    {
        MyAbility.Life--;
        if(MyAbility.Life == 0)
        {
            OnDeathDelegator(gameObject);
        }
    }
    public void Kill()
    {
        MyAbility.Life = 0;
        OnDeathDelegator(gameObject);
    }
}
