using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Photon.Voice.Unity;
using TMPro;

public class CharacterMovementHandler : NetworkBehaviour
{
    bool isRespawnRequested = false;

    //Other components
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    HPHandler hpHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;

    public TMP_Text mic_text;
    public GameObject ExitPanel;
    public int moveSpeed = 1000;
    Vector3 originPos;
    //===========================================================================
    private Animator animator;

    private bool voiceFlag = true;
    [Networked] //���콺 ��ư ��Ʈ��ũ�� üũ
    private NetworkButtons _mouseButtonsInput { get; set; }

    [Networked(OnChanged = nameof(OnRunChanged))] //run ���� ��ȭ�� �ݹ��Լ� ����
    private int _runCount { get; set; } //run ���� ���� (���� �����ϴٺ��� Count��� �״�� ����褻����)

    [Networked(OnChanged = nameof(OnShootChanged))] 
    private int _shootCount { get; set; } 
    //===========================================================================
    private void Awake()
    {
        networkCharacterControllerPrototypeCustom = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        hpHandler = GetComponent<HPHandler>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
        animator = GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        originPos = transform.position;
        print(originPos);
    }


    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            //������ �ؾ� �� ��
            if (isRespawnRequested)
            {
                Respawn();
                return;
            }

            //�׾��� ��
            if (hpHandler.isDead)
                return;
        }

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            //aim
            transform.forward = networkInputData.aimForwardVector;

            //rotation
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;

            //Move
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            networkCharacterControllerPrototypeCustom.Move(moveDirection * moveSpeed);

            //Jump
            if(networkInputData.isJumpPressed)
                networkCharacterControllerPrototypeCustom.Jump();

            //Voice
            if (networkInputData.isVoiceButtonPressed)
            {
                print("voice button pressed");
                Runner.transform.Find("Recorder").GetComponent<Recorder>().TransmitEnabled = voiceFlag;
                if (voiceFlag) mic_text.text = "Voice ON";
                else mic_text.text = "Voice OFF";
                voiceFlag = !voiceFlag;
            }
           
            

            //�������� �� Ȯ��
            CheckFallRespawn();
        }

        //===========================================================================
        var input = GetInput<NetworkInputData>(); //NetworkInputData�� ���� Ȱ���� ���� input ����
        if (input.HasValue == false) //input�� �� �ִ��� Ȯ��
            return;

        //movementInput ������ �� ��ȭ�� ������ ����(�������� ��)
        if (input.Value.movementInput != Vector2.zero) 
        {
            _runCount++;

        }else if(input.Value.movementInput == Vector2.zero) //������ ��
        {
            _runCount = 0;

        }
 

        if (input.Value.isFireButtonPressed) //�߻� ��ư ������ ��
        {
            _shootCount++;

        }else if (!input.Value.isFireButtonPressed) //�ƴ� ��
        {
            _shootCount = 0;
        }
        
        //_lastButtonsInput = input.Value.movementInput; //�ʿ���� ����
        //===========================================================================
    }

    void CheckFallRespawn()
    {
        if (transform.position.y < -12)
        {
            if (Object.HasStateAuthority)
            {
                networkInGameMessages.SendInGameRPCMessage(networkPlayer.nickName.ToString(), $"{transform.position}fell of the world");
                Respawn();
            }
        }
    }

    public void RequestRespawn()
    {
        isRespawnRequested = true;
    }

    void Respawn()
    {
        networkCharacterControllerPrototypeCustom.TeleportToPosition(originPos);

        hpHandler.OnRespawned();

        isRespawnRequested = false;
    }



    public void SetCharacterControllerEnabled(bool isEnabled)
    {
        networkCharacterControllerPrototypeCustom.Controller.enabled = isEnabled;
    }
    public static void OnRunChanged(Changed<CharacterMovementHandler> changed) //[Networked]���� ������ ��ȭ�� ���� �� �����ϴ� �ݹ��Լ�
    {
        changed.LoadOld();
        int previousRunCount = changed.Behaviour._runCount;

        changed.LoadNew();

        if (changed.Behaviour._runCount > previousRunCount)
        {
            changed.Behaviour.animator.SetFloat("Speed", 0.5f); //�ִϸ��̼� blender tree ���
            // Play jump sound/particle effect
        }
        else
        {
            changed.Behaviour.animator.SetFloat("Speed", 0); //�ִϸ��̼� blender tree ���
        }
    }

    public static void OnShootChanged(Changed<CharacterMovementHandler> changed) //[Networked]���� ������ ��ȭ�� ���� �� �����ϴ� �ݹ��Լ�
    {
        changed.LoadOld();
        int previousShootCount = changed.Behaviour._shootCount;

        changed.LoadNew();

        

        if (changed.Behaviour._shootCount > previousShootCount)
        {
            changed.Behaviour.animator.SetLayerWeight(changed.Behaviour.animator.GetLayerIndex("Shoot Layer"), 1);
            changed.Behaviour.animator.SetTrigger("Shoot"); //�ִϸ��̼� blender tree ���
            // Play jump sound/particle effect
            Debug.Log($"{changed.Behaviour._shootCount}, {previousShootCount}");
        }
        else
        {
            changed.Behaviour.animator.SetLayerWeight(changed.Behaviour.animator.GetLayerIndex("Shoot Layer"), 0); 
            //�ִϸ��̼� blender tree ���
        }
    }
}
