using System;

namespace DungeonEscape
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    public static class DungeonEvents
    {
        // Eventos de estado de juego
        public static Action<GameState> OnGameStateChanged;

        // Eventos del jugador
        public static Action<int> OnPlayerDamage;       // Envía la salud actual
        public static Action<int> OnPlayerHeal;         // Envía la salud actual
        public static Action<int> OnPlayerMaxHealthInit;// Envía la salud máxima inicial
        
        // Eventos de coleccionables y progreso
        public static Action<int> OnKeyCollected;       // Envía la cantidad recolectada en un objeto
        public static Action<int> OnKeyCountChanged;    // Envía la cantidad total acumulada de llaves del jugador
        public static Action OnDoorUnlocked;
        public static Action<int, int> OnKeysRequiredUpdate; // Envía (llaves poseídas, llaves requeridas)

        // Eventos de UI / Interacción
        public static Action<bool, string> OnInteractLook; // true/false y el texto a mostrar (ej. "Presiona E para abrir cofre")
    }
}
