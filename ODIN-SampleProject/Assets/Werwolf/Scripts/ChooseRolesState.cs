﻿using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Werwolf.Scripts
{
    public class ChooseRolesState : MonoBehaviour
    {
        [Header("Settings")] [SerializeField] private string nextState;

        [SerializeField] private int minWerwolfCount = 1;
        [SerializeField] private float werewolfPercentage = 0.33f;


        [Header("References")] [SerializeField]
        private PlayerList players;

        [SerializeField] private Roles roles;


        [SerializeField] private WerwolfStateMachine stateMachine;

        private void OnEnable()
        {
            if (PhotonNetwork.IsConnected) StartCoroutine(WaitForPlayersToSpawn());
        }


        private IEnumerator WaitForPlayersToSpawn()
        {
            int currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;

            GameObject[] playerArray = GameObject.FindGameObjectsWithTag("Player");
            while (playerArray.Length != currentRoomPlayerCount)
            {
                yield return new WaitForSeconds(0.25f);
                playerArray = GameObject.FindGameObjectsWithTag("Player");
                currentRoomPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            }


            players.All.Clear();
            players.All.AddRange(playerArray);
            List<GameObject> villagers = new List<GameObject>(playerArray);

            SelectWerewolves(ref villagers, werewolfPercentage);

            foreach (GameObject villager in villagers) SendRole(villager, RoleTypes.Villager);

            stateMachine.SwitchState(nextState);
        }

        private void SelectWerewolves(ref List<GameObject> villagers, float percentage = 0.33f)
        {
            int werwolfNum = Mathf.FloorToInt(villagers.Count * percentage);
            werwolfNum = Mathf.Max(minWerwolfCount, werwolfNum);
            for (int i = 0; i < werwolfNum; i++)
            {
                int werwolfIndex = Random.Range(0, villagers.Count);
                GameObject werewolf = villagers[werwolfIndex];

                SendRole(werewolf, RoleTypes.Werewolf);

                villagers.RemoveAt(werwolfIndex);
            }
        }

        private void SendRole(GameObject target, RoleTypes roleType)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonView photonView = target.GetComponent<PhotonView>();
                if (photonView)
                    photonView.RPC("SetRole", RpcTarget.All, roles.ToString(roleType));
            }
        }
    }
}