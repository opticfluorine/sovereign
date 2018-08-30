namespace Sovereign.EngineCore.Components
{

    /// <summary>
    /// Simplifies access to the component update interface exposed by BaseComponentCollection.
    /// </summary>
    public interface IComponentUpdater
    {

        /// <summary>
        /// Applies all pending component updates and fires the corresponding events..
        /// </summary>
        void ApplyComponentUpdates();

    }

}