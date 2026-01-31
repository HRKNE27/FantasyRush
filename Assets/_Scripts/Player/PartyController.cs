using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    [SerializeField] private GameObject _partyMemberOne;
    [SerializeField] private GameObject _partyMemberTwo;
    [SerializeField] private GameObject _partyMemberThree;
    // private Dictionary<GameObject,string> partyManager = new Dictionary<GameObject,string>();
    private Dictionary<GameObject, (PartyMemberStatus, PlayerMovement)> partyManager = new Dictionary<GameObject, (PartyMemberStatus,PlayerMovement)>();
    private int partyManagerCount;
    public static PartyController Instance;
    private bool canSwap;
    [SerializeField] private float swapCooldown;

    [SerializeField] private GameObject _virtualCamera;

    private enum PartyMemberStatus
    {
        Leader,
        Left,
        Right,
        Remainder,
        Dead
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _partyMemberOne.GetComponent<PlayerMovement>().isLeader = true;
        _partyMemberTwo.GetComponent<PlayerMovement>().isLeader = false;
        _partyMemberThree.GetComponent<PlayerMovement>().isLeader = false;

        partyManager.Add(_partyMemberOne, (PartyMemberStatus.Leader, _partyMemberOne.GetComponent<PlayerMovement>()));
        partyManager.Add(_partyMemberTwo, (PartyMemberStatus.Left, _partyMemberTwo.GetComponent<PlayerMovement>()));
        partyManager.Add(_partyMemberThree, (PartyMemberStatus.Right,_partyMemberThree.GetComponent<PlayerMovement>()));
        partyManagerCount = partyManager.Count;

        canSwap = true;
    }

    private void Start()
    {
        float horizontal = _partyMemberOne.GetComponent<PlayerMovement>().HorizontalVelocity;
        float vertical = _partyMemberOne.GetComponent<PlayerMovement>().VerticalVelocity;
        UpdateLeader(_partyMemberOne.transform.position,horizontal, vertical);
    }

    private void Update()
    {
        SwapCheck();
    }

    private void SwapCheck()
    {
        // Read the input from input manager to see who is getting swapped
        // Assign new postions for party members by switching roles

        var leaderKey = partyManager.FirstOrDefault(x => x.Value.Item1 == PartyMemberStatus.Leader).Key;
        Vector2 newPosition = leaderKey.transform.position;
        float horizontalVelocity = partyManager[leaderKey].Item2.HorizontalVelocity;
        float verticalVelocity = partyManager[leaderKey].Item2.VerticalVelocity;
        switch (partyManagerCount)
        {
            case 3:
                if (canSwap)
                {
                    
                    if (InputManager.SwapLeftPressed)
                    {
                        var leftKey = partyManager.FirstOrDefault(x => x.Value.Item1 == PartyMemberStatus.Left).Key;
                        partyManager[leaderKey] = (PartyMemberStatus.Left, partyManager[leaderKey].Item2);
                        partyManager[leftKey] = (PartyMemberStatus.Leader, partyManager[leftKey].Item2);
                        UpdateLeader(newPosition, horizontalVelocity, verticalVelocity);
                        StartCoroutine(SwapCooldown(swapCooldown));
                        return;
                    }
                    if (InputManager.SwapRightPressed)
                    {
                        var rightKey = partyManager.FirstOrDefault(x => x.Value.Item1 == PartyMemberStatus.Right).Key;
                        partyManager[leaderKey] = (PartyMemberStatus.Right, partyManager[leaderKey].Item2);
                        partyManager[rightKey] = (PartyMemberStatus.Leader, partyManager[leaderKey].Item2);
                        UpdateLeader(newPosition, horizontalVelocity, verticalVelocity);
                        StartCoroutine(SwapCooldown(swapCooldown));
                        return;
                    }              
                }     
                break;
            case 2:
                if (canSwap)
                {
                    if (InputManager.SwapLeftPressed || InputManager.SwapRightPressed)
                    {
                        var remainderKey = partyManager.FirstOrDefault(x => x.Value.Item1 == PartyMemberStatus.Remainder).Key;
                        partyManager[leaderKey] = (PartyMemberStatus.Remainder, partyManager[leaderKey].Item2);
                        partyManager[remainderKey] = (PartyMemberStatus.Leader, partyManager[leaderKey].Item2);
                        UpdateLeader(newPosition, horizontalVelocity, verticalVelocity);
                        StartCoroutine(SwapCooldown(swapCooldown));
                        return;
                    }
                }      
                break;
            case 1:
                break;
            default:
                break;
        }
    }

    private void UpdateLeader(Vector2 position,float horizontal, float vertical)
    {
        foreach (KeyValuePair<GameObject, (PartyMemberStatus, PlayerMovement)> partyMember in partyManager)
        {
            if(partyMember.Value.Item1 == PartyMemberStatus.Leader)
            {
                partyMember.Value.Item2.isLeader = true;
                partyMember.Key.SetActive(true);
                partyMember.Key.transform.position = position;
                partyMember.Value.Item2.SetVelocities(horizontal, vertical);
                _virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = partyMember.Key.transform;
                EnemyManager._instance.ChangeTarget(partyMember.Key.transform);
            }
            else
            {
                partyMember.Value.Item2.isLeader = false;
                partyMember.Key.SetActive(false);
            }
        }
    }

    public void LeaderDeathUpdate()
    {
        // If there are three members and one dies, set it so that whenever a swap is pressed it swaps to the remaining inactive character (default left)
        // If there are two members and one dies, set it so that nothing happens when a swap is pressed
        // If there is one member, gaame over

        // TODO: Update Swapcheck to account for missing members to fix errors, clean up code in PartyController and Dissolve
        switch (partyManagerCount)
        {
            case 3:
                var leaderKey = partyManager.FirstOrDefault(x => x.Value.Item1 == PartyMemberStatus.Leader).Key;
                var leftKey = partyManager.FirstOrDefault(x => x.Value.Item1 == PartyMemberStatus.Left).Key;
                var rightKey = partyManager.FirstOrDefault(x => x.Value.Item1 == PartyMemberStatus.Right).Key;
                Vector2 nextPosition = leaderKey.transform.position;
                partyManagerCount--;
                partyManager[leaderKey] = (PartyMemberStatus.Dead, partyManager[leaderKey].Item2);
                partyManager[leftKey] = (PartyMemberStatus.Leader, partyManager[leftKey].Item2);
                partyManager[rightKey] = (PartyMemberStatus.Remainder, partyManager[rightKey].Item2);
                UpdateLeader(nextPosition, 0, 0);
                break;
            case 2:
                var newLeaderKey = partyManager.FirstOrDefault(x => x.Value.Item1 == PartyMemberStatus.Leader).Key;
                var remainderKey = partyManager.FirstOrDefault(x => x.Value.Item1 == PartyMemberStatus.Remainder).Key;
                Vector2 newPosition = newLeaderKey.transform.position;
                partyManagerCount--;
                partyManager[newLeaderKey] = (PartyMemberStatus.Dead, partyManager[newLeaderKey].Item2);
                partyManager[remainderKey] = (PartyMemberStatus.Leader, partyManager[remainderKey].Item2);
                UpdateLeader(newPosition, 0, 0);
                break;
            case 1:
                Debug.Log("Game Over");
                break;
            default:
                Debug.Log("Game Over");
                break;
        }     
    }

    private IEnumerator SwapCooldown(float cooldownTime)
    {
        canSwap = false;
        yield return new WaitForSeconds(cooldownTime);
        canSwap = true;
    }
}
