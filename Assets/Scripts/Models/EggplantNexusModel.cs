namespace Models
{
    public class EggplantNexusModel
    {
        public float MaximumHealth { get; private set; }

        public float CurrentHealth { get; private set; }

        public EggplantNexusModel(float initialHealth, float maximumHealth)
        {
            CurrentHealth = initialHealth;
            MaximumHealth = maximumHealth;
        }

        public void ModifyCurrentHealth(float modification)
        {
            CurrentHealth += modification;
        }

        public void ModifyMaximumHealth(float modification)
        {
            MaximumHealth += modification;
            if (modification > 0f)
            {
                CurrentHealth += modification;
            }
            else if (CurrentHealth > MaximumHealth)
            {
                CurrentHealth = MaximumHealth;
            }
        }
    }
}

