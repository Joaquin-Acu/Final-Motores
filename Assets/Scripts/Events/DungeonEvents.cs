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
        public static Action<int> OnPlayerDamage;       
        public static Action<int> OnPlayerHeal;         
        public static Action<int> OnPlayerMaxHealthInit;
        
        // Eventos de coleccionables y progreso
        public static Action<int> OnKeyCollected;       
        public static Action<int> OnKeyCountChanged;    
        public static Action OnDoorUnlocked;
        public static Action OnChestOpened;
        public static Action<int, int> OnKeysRequiredUpdate; 

        // Eventos de UI / Interacción
        public static Action<bool, string> OnInteractLook;
    }    
}
