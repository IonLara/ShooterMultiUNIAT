using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class Jugador : NetworkBehaviour
{
    #region Parametros
    private Rigidbody _rb;
    private InfoJugador _usernamePanel;

    [Header("Movimiento")]
    private Vector3 _moveDirection = new Vector3();
    public float moveSpeed = 10;
    public float speedLimit = 10;
    
    [Header("Gravedad")]
    public float gravityNormal = 50f;
    public float gravityJump = 9.81f;
    private bool _isJumping;

    [Header("Camara")]
    public Transform transformCam;
    public float camSpeed = 10f;
    private float _pitch = 0;
    private float _yaw = 0;
    private Vector2 _camInput = new Vector2();
    public float maxPitch = 60;
    public InputAction lookAction;

    [Header("Armas")]
    public Transform transformCannon;
    [SyncVar(hook = nameof(OnKillChanged))]
    public int kills = 0;

    [Header("HP"), SyncVar(hook = nameof(HealthChanged))]
    private int hp = 5;
    private int maxHp;
    public Transform healthBar;
    [SyncVar(hook = nameof(AliveHasChanged))]
    private bool isAlive = true;

    public float respawnTime = 5;

    [Header("Nametag")]
    public TextMeshPro nametagObject;
    [SyncVar(hook = nameof(NameChanged))]
    private string username;

    [Header("Team")] 
    [SyncVar(hook = nameof(OnChangeTeam))]
    private Teams myTeam = Teams.None;

    [Header("UI")]
    public GameObject uiPanel;

    public PlayerHUD playerHUD;

    [Header("Body")]
    public GameObject[] noShowObjects = new GameObject[3];

    #endregion
    #region Unity
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        maxHp = hp;
    }
    
    void FixedUpdate()
    {
        if(!isLocalPlayer)return;
        Vector3 flat = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        Quaternion orientation = Quaternion.LookRotation(flat);
        Vector3 worldMoveDirection = orientation * _moveDirection;
        _rb.AddForce(worldMoveDirection * moveSpeed, ForceMode.Impulse);

        Vector3 horizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
        if (horizontalVelocity.magnitude > speedLimit)
        {
            Vector3 limitedVelocity = horizontalVelocity.normalized * speedLimit;
            _rb.linearVelocity = new Vector3(limitedVelocity.x, _rb.linearVelocity.y, limitedVelocity.z);
        }

        if (!_isJumping)
        {
            _rb.AddForce(Vector3.down * gravityNormal, ForceMode.Acceleration);
        }

    }
    void Update()
    {
        //_camInput = lookAction.ReadValue<Vector2>();
        _camInput = Mouse.current.delta.ReadValue();
        _yaw += _camInput.x * camSpeed * Time.deltaTime;
        _pitch += _camInput.y * camSpeed * Time.deltaTime;

        _pitch = _pitch > maxPitch ? maxPitch : _pitch < (-maxPitch) ? -maxPitch : _pitch;
        transform.eulerAngles = new Vector3(0, _yaw,0);
        transformCam.eulerAngles = new Vector3(-_pitch, transformCam.rotation.eulerAngles.y, transformCam.rotation.eulerAngles.z);
    }
    #endregion
    #region PewPew
    [Command]
    private void CommandShoot(Vector3 origen, Vector3 direccion)
    {
        if (Physics.Raycast(origen, direccion, out RaycastHit hit, 100f))
        {
            if (hit.collider.gameObject.TryGetComponent<Jugador>(out Jugador elGolpeado) == true)
            {
                if (elGolpeado.TakeDamage(1, myTeam))
                {
                    kills ++;
                }
            }
        }
    }

    private void OnKillChanged(int oldKills, int newKills)
    {
        Debug.Log("ETC...");
    }
    #endregion
    #region HP
    [Server]
    public bool TakeDamage(int amount, Teams elTeamo)
    {
        if(hp <= 0 || elTeamo == myTeam) {hp = 0;return false;}
        hp -= amount;
        if (hp <= 0)
        {
            KillPlayer();
            return true;
        }
        return false;
    }
    private void HealthChanged(int oldHealth, int newHealth)
    {
        healthBar.localScale = new Vector3(healthBar.localScale.x, (float)newHealth / maxHp, healthBar.localScale.z);
        if(isLocalPlayer)
        {
            float foo = (float)newHealth / (float)maxHp;
            playerHUD.SetHP(foo);
        }
    }
    [Server]
    private void KillPlayer()
    {
        isAlive = false;
        
    }
    private void AliveHasChanged(bool oldBool, bool newBool)
    {
        if (newBool == false)
        {
            transform.localScale = new Vector3(1, 0.3f, 1);
            transformCam.gameObject.SetActive(false);
            gameObject.GetComponent<PlayerInput>().enabled = false;
            healthBar.gameObject.SetActive(false);
            if (!isLocalPlayer) return;
            Invoke("CommandRespawn", respawnTime);
            playerHUD.gameObject.SetActive(false);
        }
        else
        {
            transform.localScale = Vector3.one;
            healthBar.gameObject.SetActive(true);
            transform.position = ShooterNetworkManager.singleton.GetStartPosition().position;

            if (!isLocalPlayer) return;
            transformCam.gameObject.SetActive(true);
            gameObject.GetComponent<PlayerInput>().enabled = true;
            playerHUD.gameObject.SetActive(true);
        }

    }
    #endregion
    #region Input
    public void Disparar(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (!context.performed)
        {
            return;
        }
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 puntoObjetivo;
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            puntoObjetivo = hit.point;
        }
        else
        {
            puntoObjetivo = ray.origin + ray.direction * 100f;
        }

        Vector3 direccion = (puntoObjetivo - transformCannon.position).normalized;

        CommandShoot(transformCannon.position, direccion);
    }
    
    public void SetMovement(InputAction.CallbackContext context)
    {
        if(!isLocalPlayer)return;
        Debug.Log("Moving");
        var dir = context.ReadValue<Vector2>().normalized;
        _moveDirection = new Vector3(dir.x, 0, dir.y);
        
    }
    
    public void ShowKills(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (context.started)
        {
            Debug.Log("Started");
            uiPanel.SetActive(true);
            CommandGetKills();
        } else if (context.canceled)
        {
            uiPanel.SetActive(false);
            Debug.Log("canceled");
        }
    }
    [Command]
    private void CommandGetKills()
    {
        string content = "";
        var info = ScoreManager.singleton.GetSortedScore();
        foreach (var item in info)
        {
            content = content + "\u2022" + item.name + " -- " + item.kills.ToString() + "<br>";
        }
        TargetShowKills(content);
    }
    [TargetRpc]
    private void TargetShowKills(string infoClear)
    {
        uiPanel.GetComponent<UIManager>().ShowKills(infoClear);
    }

    public void SetLookDirection(InputAction.CallbackContext context)
    {
        //_camInput = context.ReadValue<Vector2>();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Cursor.lockState = CursorLockMode.Locked;
        _usernamePanel = GameObject.FindGameObjectWithTag("Username").GetComponent<InfoJugador>();
        CommandChangeName(_usernamePanel.PideUsuario());
        _usernamePanel.gameObject.SetActive(false);
        CommandRegisterPlayer();
        uiPanel = FindFirstObjectByType<UIManager>(FindObjectsInactive.Include).gameObject;
        playerHUD = FindFirstObjectByType<PlayerHUD>(FindObjectsInactive.Include);
        playerHUD.gameObject.SetActive(true);
    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput.enabled = true;
        lookAction = playerInput.actions.actionMaps[0].actions[1];
        transformCam.gameObject.SetActive(true);
        nametagObject.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
        foreach (var part in noShowObjects)
        {
            part.SetActive(false);
        }
    }
    #endregion
    #region Mirror
    public override void OnStartServer()
    {
        base.OnStartServer();
        
        CommandSetTeam();
    }
    #endregion
    [Command]
    private void CommandRegisterPlayer()
    {
        ScoreManager.singleton.RegisterPlayer(this);
    }
    [Command]
    private void CommandChangeName(string maiName)
    {
        username = maiName;
    }
    private void NameChanged(string oldName, string newName)
    {
        nametagObject.text = newName;
        name = newName;
    }
    [Command]
    private void CommandRespawn()
    {
        isAlive = true;
        hp = maxHp;
    }
    [Server]
    private void CommandSetTeam()
    {
        myTeam = TeamManager.singleton.GetBalancedTeam();
        TeamManager.singleton.RegisterPlayer(this, myTeam);
        Debug.Log("WTF");
    }
    private void OnChangeTeam(Teams oldTeam, Teams newTeam)
    {
        Debug.Log("Maybe");
        SetLook(newTeam);
        
    }
    private void SetLook(Teams elTeamo)
    {
        var mat = noShowObjects[1].GetComponent<SkinnedMeshRenderer>().material;
        mat.SetFloat("_Toggle", elTeamo == Teams.Alpha ? 0 : 1);

        if (!isLocalPlayer) return;
        var miMat = transformCam.GetChild(0).GetComponent<MeshRenderer>().materials[0];
        miMat.SetFloat("_Toggle", elTeamo == Teams.Alpha ? 0 : 1);

        Debug.Log("Soy " + elTeamo.ToString() + " gurl!");
    }
}
