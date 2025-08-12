using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.Game.FeedBack
{
    public class CircularTransition : Image
    {
        public override Material materialForRendering
        {
            get
            {
                Material material = new Material(base.materialForRendering);
                material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                return material;
            }
        }
    }
}