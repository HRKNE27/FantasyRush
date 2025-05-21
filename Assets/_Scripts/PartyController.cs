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
    private Dictionary<GameObject,string> partyManager = new Dictionary<GameObject,string>();

    [SerializeField] private GameObject _virtualCamera;

    private void Awake()
    {
        _partyMemberOne.GetComponent<PlayerMovement>().isLeader = true;
        _partyMemberTwo.GetComponent<PlayerMovement>().isLeader = false;
        _partyMemberThree.GetComponent<PlayerMovement>().isLeader = false;

        partyManager.Add(_partyMemberOne, "Leader");
        partyManager.Add(_partyMemberTwo, "Left");
        partyManager.Add(_partyMemberThree, "Right");
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
        if (InputManager.SwapLeftPressed)
        {
            var leaderKey = partyManager.FirstOrDefault(x => x.Value == "Leader").Key;
            var leftKey = partyManager.FirstOrDefault(x => x.Value == "Left").Key;
            Vector2 newPosition = leaderKey.transform.position;

            float horizontalVelocity = leaderKey.GetComponent<PlayerMovement>().HorizontalVelocity;
            float verticalVelocity = leaderKey.GetComponent<PlayerMovement>().VerticalVelocity;

            partyManager[leaderKey] = "Left";
            partyManager[leftKey] = "Leader";

            UpdateLeader(newPosition, horizontalVelocity, verticalVelocity);
            return;
        }
        if (InputManager.SwapRightPressed)
        {
            var leaderKey = partyManager.FirstOrDefault(x => x.Value == "Leader").Key;
            var rightKey = partyManager.FirstOrDefault(x => x.Value == "Right").Key;
            Vector2 newPosition = leaderKey.transform.position;

            float horizontalVelocity = leaderKey.GetComponent<PlayerMovement>().HorizontalVelocity;
            float verticalVelocity = leaderKey.GetComponent<PlayerMovement>().VerticalVelocity;

            partyManager[leaderKey] = "Right";
            partyManager[rightKey] = "Leader";

            UpdateLeader(newPosition,horizontalVelocity,verticalVelocity);
            return;
        }
        
    }

    private void UpdateLeader(Vector2 position,float horizontal, float vertical)
    {
        foreach (KeyValuePair<GameObject, string> partyMember in partyManager)
        {
            if(partyMember.Value == "Leader")
            {
                partyMember.Key.GetComponent<PlayerMovement>().isLeader = true;
                partyMember.Key.SetActive(true);
                partyMember.Key.transform.position = position;
                partyMember.Key.GetComponent<PlayerMovement>().SetVelocities(horizontal, vertical);
                _virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = partyMember.Key.transform;
                EnemyManager._instance.ChangeTarget(partyMember.Key.transform);
            }
            else
            {
                partyMember.Key.GetComponent<PlayerMovement>().isLeader = false;
                partyMember.Key.SetActive(false);
            }
        }
    }

    private GameObject GetLeader()
    {
        return partyManager.FirstOrDefault(x => x.Value == "Leader").Key;
    }
}
