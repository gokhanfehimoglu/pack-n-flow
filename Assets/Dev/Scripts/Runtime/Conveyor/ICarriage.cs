namespace PackNFlow
{
    public interface ICarriage
    {
        bool IsReady { get; }
        bool HasCompletedPath { get; }
        Unit Occupant { get; }
        void SnapToEnd();
        void AssignUnit(Unit unit);
        void ReleaseUnit();
        void OnUnitDepleted();
    }
}
