using UnityEngine;
using MixedReality.Toolkit.UX;

public class DialogController : MonoBehaviour
{
    public DialogPool dialogPool;  // Reference to the Dialog Pool component
    private IDialog currentDialog; // Store the currently active dialog

    public void ShowInstructions()
    {
        if (dialogPool == null)
        {
            Debug.LogError("Dialog Pool is not assigned in the Inspector!");
            return;
        }

        // Get a dialog instance from the pool
        currentDialog = dialogPool.Get();

        if (currentDialog != null)
        {
            currentDialog
                .SetHeader("Game Instructions") // Title
                .SetBody("1. Select a circuit.\n2. Select a car.\n3. Press Start to race!\n4. Use controls to navigate." +
                "\n5. If you pick up all the cherries you win.\n6. If your score is < -2 you lose.") // Message
                .SetPositive("OK", CloseDialog) // OK Button closes the dialog
                .Show();
        }
    }

    private void CloseDialog(DialogButtonEventArgs args)
    {
        if (currentDialog != null)
        {
            Debug.Log("Instructions Dialog Closed.");
            currentDialog.Dismiss(); // Hide the dialog
            currentDialog = null;
        }
    }
}