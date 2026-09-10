using System.Collections;
using UnityEngine;

public class OrganPurchaseHover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OrganPurchaseController purchaseController;

    [Header("Close Delay")]
    [SerializeField] private float closeDelay = 0.25f;

    private bool pointerOnOrgan;
    private bool pointerOnPanel;

    private Coroutine closeRoutine;

    public void EnterOrgan()
    {
        pointerOnOrgan = true;

        if (purchaseController != null)
        {
            purchaseController.SetPointerOnOrgan(true);
        }

        CancelClose();

        if (purchaseController != null)
        {
            purchaseController.OpenPanel();
        }
    }

    public void ExitOrgan()
    {
        pointerOnOrgan = false;

        if (purchaseController != null)
        {
            purchaseController.SetPointerOnOrgan(false);
        }

        StartCloseCheck();
    }

    public void EnterPanel()
    {
        pointerOnPanel = true;

        CancelClose();
    }

    public void ExitPanel()
    {
        pointerOnPanel = false;

        StartCloseCheck();
    }

    private void StartCloseCheck()
    {
        CancelClose();

        closeRoutine = StartCoroutine(
            CloseAfterDelay()
        );
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSecondsRealtime(
            closeDelay
        );

        if (!pointerOnOrgan &&
            !pointerOnPanel &&
            purchaseController != null)
        {
            purchaseController.ClosePanel();
        }

        closeRoutine = null;
    }

    private void CancelClose()
    {
        if (closeRoutine == null)
        {
            return;
        }

        StopCoroutine(closeRoutine);
        closeRoutine = null;
    }
}