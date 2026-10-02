using DogHeist.UI.Hud;
using DogHeist.UI.Screens;
using UnityEngine;

namespace DogHeist.EditorTools.Greybox
{
    internal sealed class GreyboxHud
    {
        public Canvas Canvas { get; set; }

        public NoiseMeterUI NoiseMeter { get; set; }

        public ThiefStatusUI Status { get; set; }

        public ResultScreenUI ResultScreen { get; set; }
    }
}
