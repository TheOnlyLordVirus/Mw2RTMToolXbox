namespace LordVirusMw2XboxLib;

internal interface IGameCheat
{
    bool GetValue();

    void Enable();
    void Disable();
    void Toggle();
}