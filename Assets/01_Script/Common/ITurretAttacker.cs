// ITurretAttacker.cs
// Interfaz que todos los enemigos que atacan la torreta deben implementar.
// Responsable: Arandia (integración)

using LastMachine.Arandia;
using UnityEngine;

namespace LastMachine
{
    /// <summary>
    /// Contrato común para cualquier enemigo que quiera dañar componentes de la torreta.
    /// </summary>
    public interface ITurretAttacker
    {
        void AttackTurret(TurretDegradation_Arandia turretDegradation);
    }
}