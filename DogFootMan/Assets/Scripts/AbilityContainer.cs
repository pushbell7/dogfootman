using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AbilityContainer : MonoBehaviour
{
    [System.Serializable]
    public struct Ability
    {
        public float Power;
        public float Mass;
        public float MaxSpeed;

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
                return ability;
            }
        }

        class KickboardFactory : BaseFactory
        {
            public override Ability Make()
            {
                var ability = new Ability();
                ability.Power = 10000.0f;
                ability.Mass = 5.0f;
                ability.MaxSpeed = 8.0f;
                return ability;
            }
        }

        public Ability Make(string type)
        {
            if (type.Contains("Car")) { return new CarFactory().Make(); }
            else if (type.Contains("Human")) { return new HumanFactory().Make(); }
            else if(type.Contains("MainCharacter")) { return new HumanFactory().Make(); }
            else if (type.Contains("ItemToRide")) { return new KickboardFactory().Make(); }
            else { return new BaseFactory().Make(); }
        }
    }
    Ability MyAbility;


    // Start is called before the first frame update
    void Start()
    {
        MyAbility = new DefaultAbilityFactory().Make(gameObject.name);
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
}
