using UnityEngine;

public class CheatMenu : Menu
{
    protected override void OnStart()
    {
        key = "t";
    }

    private void Teleport(Vector3 to) {
        GameInfo.ControlledBlob.Teleport(to);
        HideMenu();
    }

    public void BallsButton()
    {
        Teleport(new Vector3(47, 30, 47));
    }

    public void Spinner1Button()
    {
        Teleport(new Vector3(-7, 30, 47));
    }

    public void ClimbingWallButton()
    {
        Teleport(new Vector3(-47, 30, 27));
    }

    public void PushingWallButton()
    {
        Teleport(new Vector3(-40, 40, 25));
    }

    public void WreckingBallsButton()
    {
        Teleport(new Vector3(-8, 45, 17));
    }

    public void Spinner2Button()
    {
        Teleport(new Vector3(-32, 40, -7));
    }

    public void SliderButton()
    {
        Teleport(new Vector3(-47, 35, -47));
    }

    public void HoopsButton()
    {
        Teleport(new Vector3(-3, 40, -46));
    }

    public void SlidingPlatformsButton()
    {
        Teleport(new Vector3(32, 40, -46));
    }

    public void SpinningWallButton() {
        Teleport(new Vector3(47, 40, -28));
    }

    public void RotatingPlatformsButton() {
        Teleport(new Vector3(15, 30, -22));
    }

    public void CannonButton() {
        Teleport(new Vector3(22, 45, 9));
    }

}
