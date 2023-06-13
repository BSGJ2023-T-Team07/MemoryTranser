namespace MemoryTranser.Scripts.Game.Fairy {
    public enum FairyState {
        //待機状態
        IdlingWithoutMemory,

        //記憶を持って待機
        IdlingWithMemory,

        //持たないで歩いてる
        WalkingWithoutMemory,

        //記憶を持って歩いてる
        WalkingWithMemory,

        //構え状態
        Standby,

        //操作不能状態
        Freeze
    }
}