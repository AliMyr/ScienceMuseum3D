using UnityEngine;
using ScienceMuseum.Core;

namespace ScienceMuseum.Exhibits
{
    public class TestExhibit : ExhibitBase
    {
        public override void OnActivate()
        {
            Debug.Log($"[TestExhibit] Активирован экспонат: {Title}");
        }
    }
}