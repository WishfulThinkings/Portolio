using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;



public class MeetingRoomChair : NetworkBehaviour, InterfaceInteractable
{
    [SerializeField]
    private string prompt;
    public string interactionPrompt => prompt;

    [SerializeField]
    private Transform offset;

    public bool isOccupied;

    [Header("SFX")]
    [SerializeField]
    private string sfxPlay;

    public bool Interact(Interactor interactor)
    {
        ToogleInteraction();
        return true;
    }
    private void Start()
    {
        isOccupied = false;

        //StarterAssets.ThirdPersonController.Instance.AssignAnimationIDs();
    }
    private void Update()
    {
        if (StarterAssets.StarterAssetsInputs.Instance != null)
        {
            if (StarterAssets.StarterAssetsInputs.Instance.sit)
            {
                StarterAssets.StarterAssetsInputs.Instance.jump = false;
            }
            else if (!StarterAssets.StarterAssetsInputs.Instance.sit)
            {
                //StarterAssets.StarterAssetsInputs.Instance.jump = true;
            }
        }

    }
    private void ToogleInteraction()
    {
        if (!isOccupied && !StarterAssets.StarterAssetsInputs.Instance.sit && StarterAssets.ThirdPersonController.Instance.Grounded)
        {
            //movement
            StarterAssets.StarterAssetsInputs.Instance.StopMovement();

            //deactivate controller
            StarterAssets.ThirdPersonController.Instance._controller.enabled = false;

            //Set player to offset position
            StarterAssets.ThirdPersonController.Instance._controller.transform.position = offset.transform.position;
            StarterAssets.ThirdPersonController.Instance._controller.transform.rotation = offset.transform.rotation;

            //animation
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDSit, true);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDSitClap, false);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDSitWave, false);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDStandClap, false);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDStandWave, false);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDStandDance, false);

            //deactivate UIPanel
            InteractionPromptUI.Instance._uiPanel.SetActive(false);

            //Play SFX
            AudioManager.Instance.PlaySFX(sfxPlay);

            isOccupied = true;
            StarterAssets.StarterAssetsInputs.Instance.sit = true;
        }
        else if (isOccupied && StarterAssets.StarterAssetsInputs.Instance.sit && StarterAssets.ThirdPersonController.Instance.Grounded)
        {
            //movement
            StarterAssets.StarterAssetsInputs.Instance.StartMovement();

            //activate controller
            StarterAssets.ThirdPersonController.Instance._controller.enabled = true;

            //Set player to offset position
            StarterAssets.ThirdPersonController.Instance._controller.transform.position = offset.transform.position;
            StarterAssets.ThirdPersonController.Instance._controller.transform.rotation = offset.transform.rotation;

            //animation
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDSit, false);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDSitClap, false);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDSitWave, false);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDStandClap, false);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDStandWave, false);
            StarterAssets.ThirdPersonController.Instance._animator.SetBool(StarterAssets.ThirdPersonController.Instance._animIDStandDance, false);

            //deactivate UIPanel
            InteractionPromptUI.Instance._uiPanel.SetActive(true);

            //Play SFX
            AudioManager.Instance.PlaySFX(sfxPlay);

            isOccupied = false;
            StarterAssets.StarterAssetsInputs.Instance.sit = false;
        }
    }
}