namespace MemoryTranser.Scripts.Game.Fairy {
    public enum FairyState {
        //待機状態
        IdlingWithoutBox,

        //記憶を持って待機
        IdlingWithBox,

        //持たないで歩いてる
        WalkingWithoutBox,

        //記憶を持って歩いてる
        WalkingWithBox,

        //操作不能状態
        Freeze
    }
}