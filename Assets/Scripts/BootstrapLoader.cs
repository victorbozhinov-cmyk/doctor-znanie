using UnityEngine;

public class BootstrapLoader : MonoBehaviour
{
    private void Start()
    {
        // Вече НЕ зареждаме MainMenu автоматично.
        //
        // Bootstrap остава активната начална сцена,
        // докато потребителят не влезе или не се регистрира.
        //
        // След успешен Login/Register:
        // AuthValidationController отваря MainMenu.
    }
}