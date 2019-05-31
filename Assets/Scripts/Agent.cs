﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public bool isAi;
    public int amount;
    Vector3 moveDir = Vector3.zero;
    Color color;

    //for player
    public float speed;
    CharacterController controller;

    //for ai
    NavMeshAgent agent;

    private void Start()
    {
        amount = 1;

        //Set color
        color = Random.ColorHSV();
        color.a = .4f;

        gameObject.GetComponentInChildren<Renderer>().material.SetColor("_Color", color);
        gameObject.GetComponentInChildren<Renderer>().material.SetColor("_OutlineColor", color);

        if (!isAi)
        {
            controller = GetComponent<CharacterController>();
        }
        else
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        if (!isAi)
        {
            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical")).normalized;
            moveDir *= speed;

            controller.Move(moveDir * Time.deltaTime);

            if (moveDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(moveDir);
        }
        else
        {
            agent.SetDestination(moveDir);
            SetDir();
        }

        CheckCollider();
    }

    void SetDir()
    {
        if (agent.velocity == Vector3.zero)
        {
            moveDir = transform.position + Random.insideUnitSphere * Random.Range(10, 40);
        }
    }

    public void AddAmount(int number)
    {
        amount += number;
    }

    public void CheckCollider()
    {
        foreach (Collider col in Physics.OverlapSphere(transform.position, 2))
        {
            if (col.gameObject.tag == "Npc" && col.GetComponent<NpcController>().player != gameObject)
            {
                AddNpc(col.gameObject);
            }
            else if (col.gameObject.tag == "Player" && col.GetComponent<Agent>().amount < amount)
            {
                KillPlayer(col.gameObject);
            }
        }
    }

    public void AddNpc(GameObject npc)
    {
        NpcController npcCont = npc.GetComponent<NpcController>();
        if (!npcCont.isFollowing)
        {
            npcCont.AddPlayer(gameObject);
            AddAmount(1);
        }
        else if (npcCont.isFollowing && npcCont.player != gameObject)
        {
            if (npcCont.player.GetComponent<Agent>().amount < amount)
            {
                npcCont.player.GetComponent<Agent>().AddAmount(-1);
                npcCont.AddPlayer(gameObject);
                AddAmount(1);
            }
        }
    }

    public void KillPlayer(GameObject player)
    {
        if (player.GetComponent<Agent>().amount == 1)
        {
            GameManager.GM.DestroyPlayer(player);
            Debug.Log("Player was killed");
            GameManager.GM.SpawnFollowers(gameObject);
        }
    }
}
