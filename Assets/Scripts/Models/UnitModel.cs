namespace Models
{
    public class UnitModel
    {
        public float Speed { get; private set; }

        public float Damage { get; private set; }

        public float AttackSpeed { get; private set; }

        public float Health { get; private set; }

        public UnitModel(float speed, float damage, float attackSpeed, float health)
        {
            Speed = speed;
            Damage = damage;
            AttackSpeed = attackSpeed;
            Health = health;
        }

        public void ModifySpeed(float multiplier)
        {
            Speed *= multiplier;
        }

        public void ModifyDamage(float amount)
        {
            Damage += amount;
        }

        public void ModifyAttackSpeed(float amount)
        {
            AttackSpeed += amount;
        }

        public void ModifyHealth(float amount)
        {
            Health += amount;
        }
    }
}