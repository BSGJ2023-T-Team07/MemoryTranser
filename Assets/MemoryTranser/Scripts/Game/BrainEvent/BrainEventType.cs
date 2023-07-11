namespace MemoryTranser.Scripts.Game.BrainEvent {
    public enum BrainEventType {
        None,

        //MemoryBoxの文字部分が煙で見えなくなる
        Blind,

        //煩悩が大量発生する
        DesireOutbreak,

        //操作が逆転する
        InvertControl,

        //一部のMemoryBoxが直近のフェイズ教科を使って再抽選される
        AchievementOfStudy,

        //フィーバータイム
        FeverTime,

        //以上の要素の数を取得できる
        Count
    }
}