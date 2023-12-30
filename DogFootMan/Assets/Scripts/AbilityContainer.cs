using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class Ability
{
    public enum EItemType
    {
        NoItem,
        RidingItem,
    }

    float power;
    public float Power
    {
        get
        {
            return bIsBoosted ? power * 3 : power;
        }
        set
        {
            power = value;
        }
    }
    public float Mass;
    float maxSpeed;
    public float MaxSpeed
    {
        get
        {
            return bIsBoosted ? maxSpeed * 3 : maxSpeed;
        }
        set
        {
            maxSpeed = value;
        }
    }
    public float MaxStamina;
    float currentStamina;
    public float CurrentStamina
    {
        get
        {
            return currentStamina;
        }
        set
        {
            currentStamina = MaxStamina < value ? MaxStamina : value;
            if (currentStamina < 0)
            {
                bIsBoosted = false;
            }
        }
    }
    public int Life;
    public EItemType Type;
    public bool bIsBoosted;

    public void Add(Ability other)
    {
        if (other == null) return;

        Power += other.Power;
        Mass += other.Mass;
        MaxSpeed += other.MaxSpeed;
        Life += other.Life;
    }
    public void Remove(Ability other)
    {
        if (other == null) return;

        Power -= other.Power;
        Mass -= other.Mass;
        MaxSpeed -= other.MaxSpeed;
        //Life -= other.Life;
    }
}

public class AbilityContainer : MonoBehaviour
{
    public static class DefaultAbilityFactory
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
                ability.CurrentStamina = ability.MaxStamina = 100.0f;
                return ability;
            }
        }
        class CarFactory : BaseFactory
        {
            public override Ability Make()
            {
                var ability = new Ability();
                ability.Power = 6000.0f;
                ability.Mass = 20.0f;
                ability.MaxSpeed = 20.0f;
                ability.Life = 10;
                return ability;
            }
        }

        class KickboardFactory : BaseFactory
        {
            public override Ability Make()
            {
                var ability = new Ability();
                ability.Power = 300.0f;
                ability.Mass = 4.0f;
                ability.MaxSpeed = 4.0f;
                ability.Life = 0;
                ability.Type = Ability.EItemType.RidingItem;
                return ability;
            }
        }


        public static Ability Make(ObjectManager.ObjectType type)
        {
            if (type == ObjectManager.ObjectType.Car) { return new CarFactory().Make(); }
            else if (type == ObjectManager.ObjectType.Human) { return new HumanFactory().Make(); }
            else if(type == ObjectManager.ObjectType.MyCharacter) { return SharedInfo.Get().MyAbility; }
            else if (type == ObjectManager.ObjectType.ItemToRide) { return new KickboardFactory().Make(); }
            else { return new BaseFactory().Make(); }
        }
    }
    Ability MyAbility;

    public delegate void OnDeath(GameObject other);
    public OnDeath OnDeathDelegator;

    Dictionary<Ability.EItemType, Ability> ItemContainer;

    // Start is called before the first frame update
    void Start()
    {
        MyAbility = DefaultAbilityFactory.Make(ObjectManager.GetType(gameObject));
        ItemContainer = new Dictionary<Ability.EItemType, Ability>();
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

    public void SetBoostMode(bool bIsBoosted)
    {
        MyAbility.bIsBoosted = bIsBoosted;
    }
    public void AdjustStamina(float delta)
    {
        MyAbility.CurrentStamina += delta;
    }
    public float GetCurrentStamina()
    {
        return MyAbility.CurrentStamina;
    }
    public void GetItem(AbilityContainer other)
    {
        if (other.MyAbility.Type != Ability.EItemType.NoItem)
        {
            var otherAbility = other.MyAbility;
            var item = ItemContainer.GetValueOrDefault(otherAbility.Type);
            MyAbility.Remove(item);
            MyAbility.Add(otherAbility);

            ItemContainer.Remove(otherAbility.Type);
            ItemContainer.Add(otherAbility.Type, otherAbility);
        }
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

    public int GetLife()
    {
        return MyAbility.Life;
    }

    public float GetMaxStamina()
    {
        return MyAbility.MaxStamina;
    }
}
