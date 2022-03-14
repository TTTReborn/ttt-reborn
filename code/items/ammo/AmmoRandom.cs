using System;
using System.Collections.Generic;

using Sandbox;

namespace TTTReborn.Items
{
    [Library("ttt_ammo_random")]
    public class AmmoRandom : Entity
    {
        /// <summary>
        /// Spawn weapons of a specific category.
        /// </summary>
        [Property(Title = "Spawn ammo of a specific weapon category")]
        public CarriableCategories? Category { get; set; } = null;

        public void Activate()
        {
            List<Type> ammoTypes = Utils.GetTypesWithAttribute<Ammo, SpawnableAttribute>();

            if (Category != null)
            {
                List<Type> filteredTypes = new();

                // Just spawn ammo that has spawnable weapons
                List<Type> wepTypes = Utils.GetTypesWithAttribute<Weapon, SpawnableAttribute>();

                foreach (Type ammoType in ammoTypes)
                {
                    foreach (Type wepType in wepTypes)
                    {
                        WeaponAttribute weaponAttribute = Utils.GetAttribute<WeaponAttribute>(wepType);

                        if (weaponAttribute != null && weaponAttribute.Category == Category && weaponAttribute.PrimaryAmmoType == ammoType)
                        {
                            if (!filteredTypes.Contains(ammoType))
                            {
                                filteredTypes.Add(ammoType);
                            }
                        }
                    }
                }

                ammoTypes = filteredTypes;
            }

            if (ammoTypes.Count <= 0)
            {
                return;
            }

            Type typeToSpawn = ammoTypes[Utils.RNG.Next(ammoTypes.Count)];

            Ammo ent = Utils.GetObjectByType<Ammo>(typeToSpawn);
            ent.Position = Position;
            ent.Rotation = Rotation;
            ent.Spawn();
        }
    }
}
