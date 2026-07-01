// Esempio cAlgo C#
using cAlgo.API;

namespace cAlgo
{
    [Robot(AccessRights = AccessRights.None)]
    public class NewRobot : Robot
    {
        protected override void OnStart()
        {
            Print('OmniHub Connesso');
        }
    }
}