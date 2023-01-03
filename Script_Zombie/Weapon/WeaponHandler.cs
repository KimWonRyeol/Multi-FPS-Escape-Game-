using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Text;
using UnityEngine.UI;
using TMPro;


public class WeaponHandler : NetworkBehaviour
{

    [Header("Effects")]
    public ParticleSystem fireParticleSystem;
    public AudioClip audio; //����� Ŭ��, �� �߻� �Ҹ� �ֱ� ���ؼ�
    public AudioSource audioSource;

    [Header("Aim")]
    public Transform aimPoint;

    [Header("Collision")]
    public LayerMask collisionLayers;


    [Networked(OnChanged = nameof(OnFireChanged))]
    public bool isFiring { get; set; }


    float lastTimeFired = 0;


    //Other components
    HPHandler hpHandler;
    NetworkPlayer networkPlayer;
    NetworkObject networkObject;

    //refactoring �� ����d
    private bool keyFlag = false; //��¥ ��¥ ���� �Ǵܿ� ����
    private bool hasKey = false; //���� ���� Ȯ�ο� ����

    //���� ������ ���� ������
    private StringBuilder stringBuilder = new StringBuilder();
    private string strArrayToString;
    private int puzzleCount = 0;

    public GameObject ItemGetUI; //������ ȹ�� UI
    public TMP_Text tmp; //������ ȹ�� UI������ �ؽ�Ʈ�� �����ϱ� ���� ����

    private bool hasOil = false; //�⸧ ���� Ȯ�ο� ����

    private float clickTime; //�ð���
    private bool isClickBoat = false; //��Ʈ ������ ������ Ȯ�ο� ����

    public GameObject hintPanel; //��Ʈ �г� ������Ʈ
    public TMP_Text hintText; //��Ʈ �ؽ�Ʈ ������Ʈ
    private string[] arrHint = new string[] {
        "|  ���۹�  |\n\n\n1. W/A/S/D �Ǵ� ����Ű�� ĳ���͸� ������ �� �ִ�.2. Space�ٸ� ���� ������ �� �� �ִ�.3. V�� ���� ��Ƽ ������ �Ѱ� �� �� �ִ�.\n4. F�� ���� ��ȣ�ۿ� �� �� �ִ�.",
        "|  �ź��� �ϱ� 1  |\n\n\n19xx 10�� 12��\n\n���� ���� ������ ��Ű�� ���ؼ� ������ ���� �ݴ� Ư���� ��ġ�� �������.\n�� ��ġ�� ������ �ƴ� ������� ���谡 �Ǿ���\n�� ��ġ�� Ǯ�� ���ؼ��� ���� ���� ������� ��ư�� ������ �Ѵ�.\n�ٵ� �̰� ���� ��� ��������� ������.....\n\n\n\n(���翡 �ܼ��� ���� �� ����)",
        "|  �ź��� �ϱ� 2  |\n\n\n19xx 10�� 14��\n\n���� �鸮�� �ҹ����δ� �� ������ ������ ���� Ż���ߴٴ���\n����� �˸� ���� Ż���� �� �����ٵ�....\n��� ���� ������� ������ �׷��� ���Ƹ԰� �� ��Ȳ�� �Ǿ�� �����̶��\n�и� ���� ���� Ż���� �ܼ��� ���� �ž�.....\n�׷��� ��ü ��� �������ɱ�...?\n\n\n\n(�������� ���� ū ���� �ܼ��� ���� �� ����)",
        "|  ������ �ϱ�  |\n\n\n19xx 10�� 8��\n\n���� ���� �������� ������ϴ���.... ���� ��¥ �ֱ� �Ѱǰ�?\nȤ�� �𸣴ϱ� �� ���� ������ ������ �� �ֵ��� �غ��� �־߰ھ�.\n���п� �⸧�� �̷ο� ���ܵ־߰ھ�",
        "�� �۵� ��ġ�� ���� �ܼ�\n\n1. (0, 0, 1)\n2. (1, 0, 0)\n3. (0, 1, 0)\n4. (1, 1, 1)"
    };

    private void Awake()
    {
        hpHandler = GetComponent<HPHandler>();
        networkPlayer = GetBehaviour<NetworkPlayer>();
        networkObject = GetComponent<NetworkObject>();
        audioSource = GetComponentInChildren<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }
    private void Update()
    {
        if (isClickBoat)
        {
            clickTime += Time.deltaTime;
            Debug.Log($"Time : {clickTime}");
        }
        else
        {
            clickTime = 0;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (hpHandler.isDead)
            return;

        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.isFireButtonPressed)
                Fire(networkInputData.aimForwardVector);

            if (networkInputData.isInteractButtonPressed)
                Interact(networkInputData.aimForwardVector);

            if (networkInputData.isEngineButtonPressed)
            {
                Engine(networkInputData.aimForwardVector);
                isClickBoat = true;
            }
            else
            {
                isClickBoat = false;
            }
        }
    }


    void Interact(Vector3 aimForwardVector)
    {
        Runner.LagCompensation.Raycast(
            aimPoint.position,
            aimForwardVector,
            100,
            Object.InputAuthority,
            out var hitinfo,
            collisionLayers,
            HitOptions.IncludePhysX/*IgnoreInputAuthority*/);

        float buttonDistance = 4;


        if (hitinfo.Distance < buttonDistance)
        {
            if (hitinfo.Collider != null)
            {
                //tmp.text = "";
                Debug.Log($"{Time.time} {transform.name} hit collider {hitinfo.Collider}");

                //Button
                if (hitinfo.Collider.CompareTag("DoorButton"))
                    hitinfo.Collider.GetComponent<ButtonController>().isOpenAnimation = true;

                //Hint
                if (hitinfo.Collider.CompareTag("0")
                    || hitinfo.Collider.CompareTag("1")
                    || hitinfo.Collider.CompareTag("2")
                    || hitinfo.Collider.CompareTag("3")
                    || hitinfo.Collider.CompareTag("4"))
                    ShowHint(hitinfo);

                //Recovery item
                if (hitinfo.Collider.CompareTag("RecoveryItem"))
                    transform.gameObject.GetComponent<HPHandler>().GetRecoveryItem(5);

                //Oil
                if (hitinfo.Collider.CompareTag("Oil"))
                    GetOil(hitinfo);

                //Boat
                if (hitinfo.Collider.CompareTag("Boat"))
                {
                    if (!hasOil)
                    {
                        tmp.text = "�۵����� �ʽ��ϴ�.";
                        StartCoroutine("FadeInOutUI");
                    }
                    else
                    {
                        tmp.text = "EŰ�� 5���̻� ������ �մϴ�.";
                        StartCoroutine("FadeInOutUI");
                    }
                }

                //OrderedButton
                if (hitinfo.Collider.CompareTag("O")
                    || hitinfo.Collider.CompareTag("P")
                    || hitinfo.Collider.CompareTag("E")
                    || hitinfo.Collider.CompareTag("N"))
                    CheckOrder(hitinfo.Collider.tag, hitinfo.GameObject);

                //Key
                if (!hasKey)
                {
                    if (hitinfo.Collider.CompareTag("TrueKey"))
                        GetKey(hitinfo, true);
                    if (hitinfo.Collider.CompareTag("FalseKey"))
                        GetKey(hitinfo, false);
                }
                else if (hasKey)
                {
                    UseKey(hitinfo, keyFlag);
                }
            }
        }

    }

    void ShowHint(LagCompensatedHit hitObject)
    {
        hintPanel.SetActive(true);
        int index = int.Parse(hitObject.Collider.tag);
        hintText.text = arrHint[index];
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    void CheckOrder(string tag, GameObject door)
    {
        if (stringBuilder.ToString().Contains(tag)) return;

        stringBuilder.Append(tag);
        puzzleCount++;

        strArrayToString = stringBuilder.ToString();
        print("���� ����:" + strArrayToString);
        if (strArrayToString == "OPEN")
        {
            door.GetComponent<ButtonController>().isOpenAnimation = true;
            tmp.text = "�����Դϴ�!!";
            StartCoroutine("FadeInOutUI");
        }
        else if (puzzleCount == 4)
        {
            tmp.text = "�߸��� �Է��Դϴ�. 3�� �� �ٽ� �õ��� �� �ֽ��ϴ�.";
            StartCoroutine("FadeInOutUI");
            StartCoroutine("LockButton");
            //stringBuilder = new StringBuilder();
            puzzleCount = 0;
        }
    }

    IEnumerator LockButton()
    {

        yield return new WaitForSeconds(3.0f);
        stringBuilder = new StringBuilder();
    }
    void UseKey(LagCompensatedHit hitObject, bool isTrue)
    {
        //���� ���� ���¿��� ���� �� ȹ���Ϸ��� �ҽ�
        if (hitObject.Collider.CompareTag("FalseKey") || hitObject.Collider.CompareTag("TrueKey"))
        {
            tmp.text = "�̹� ���踦 �����ϰ� �ֽ��ϴ�.";
            StartCoroutine("FadeInOutUI");
        }

        if (hitObject.Collider.CompareTag("Door"))
        {
            //��¥ ���� ���
            if (isTrue)
            {
                hitObject.GameObject.GetComponent<ButtonController>().isOpenAnimation = true;
                hasKey = false;
                tmp.text = "���踦 ����߽��ϴ�.";
                StartCoroutine("FadeInOutUI");
            }
            //��¥ ���� ���
            else
            {
                hasKey = false;
                tmp.text = "���� ���ۿ� ���� �ʽ��ϴ�.";
                StartCoroutine("FadeInOutUI");
            }
        }
    }
    void GetKey(LagCompensatedHit hitObject, bool isTrue)
    {
        keyFlag = isTrue;
        hasKey = true;
        hitObject.GameObject.GetComponent<ObjectHandler>().isDestroy = true;
        tmp.text = "���� ���踦 ȹ���߽��ϴ�.";
        StartCoroutine("FadeInOutUI");
    }
    void GetOil(LagCompensatedHit hitObject)
    {
        hasOil = true;
        hitObject.GameObject.GetComponent<ObjectHandler>().isDestroy = true;
        tmp.text = "��Ʈ�� �⸧�� ȹ���߽��ϴ�.";
        StartCoroutine("FadeInOutUI");
    }

    void Fire(Vector3 aimForwardVector)
    {
        audioSource.clip = audio;

        //Limit fire rate
        if (Time.time - lastTimeFired < 0.15f)
            return;

        StartCoroutine(FireEffectCO());

        Runner.LagCompensation.Raycast(
            aimPoint.position,
            aimForwardVector,
            100,
            Object.InputAuthority,
            out var hitinfo,
            collisionLayers,
            HitOptions.IncludePhysX/*IgnoreInputAuthority*/);

        float hitDistance = 23;

        if (hitinfo.Distance < hitDistance)
        {
/*            if (hitinfo.Hitbox != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit hitbox {hitinfo.Hitbox.transform.root.name}");

                if (Object.HasStateAuthority)
                    hitinfo.Hitbox.transform.root.GetComponent<HPHandler>().OnTakeDamage(networkPlayer.nickName.ToString(), 1);

            }*/
            if (hitinfo.Collider != null)
            {
                Debug.Log($"{Time.time} {transform.name} hit collider {hitinfo.Collider}");

                if (hitinfo.Collider.CompareTag("Enemy"))
                    hitinfo.Collider.GetComponent<EnemyFSM>().HitEnemy(5);
            }
        }
        lastTimeFired = Time.time;
    }

    void Engine(Vector3 aimForwardVector)
    {
        Runner.LagCompensation.Raycast(
            aimPoint.position,
            aimForwardVector,
            100,
            Object.InputAuthority,
            out var hitinfo,
            collisionLayers,
            HitOptions.IncludePhysX/*IgnoreInputAuthority*/);

        float buttonDistance = 10;

        if (hitinfo.Distance > 0)
            buttonDistance = hitinfo.Distance;


        if (hitinfo.Collider != null)
        {
            if (hitinfo.Collider.CompareTag("Boat"))
            {
                if (!hasOil)
                {
                    tmp.text = "�۵����� �ʽ��ϴ�.";
                    StartCoroutine("FadeInOutUI");
                }
                else
                {
                    if (clickTime >= 5)
                    {
                        hitinfo.GameObject.GetComponent<ShowClearUI>().isShow = true;
                        //Time.timeScale = 0f;
                    }
                }
            }
        }
    }

    IEnumerator FireEffectCO()
    {
        isFiring = true;

        if (Object.HasInputAuthority)
        {
            if (!fireParticleSystem.isPlaying)
            {
                fireParticleSystem.Play();
            }
            if (!audioSource.isPlaying)
            {
                networkObject.GetComponent<NetworkInGameMessages>().PlayAudio(audio);
            }
        }

        yield return new WaitForSeconds(0.09f);

        isFiring = false;
    }

    IEnumerator FadeInOutUI()
    {
        ItemGetUI.SetActive(true);
        yield return new WaitForSeconds(4.0f);
        ItemGetUI.SetActive(false);
    }

    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        bool isFiringCurrent = changed.Behaviour.isFiring;

        //Load the old value
        changed.LoadOld();

        bool isFiringOld = changed.Behaviour.isFiring;

        if (isFiringCurrent && !isFiringOld)
            changed.Behaviour.OnFireRemote();

    }


    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
            networkObject.GetComponent<NetworkInGameMessages>().PlayAudio(audio);
    }
}