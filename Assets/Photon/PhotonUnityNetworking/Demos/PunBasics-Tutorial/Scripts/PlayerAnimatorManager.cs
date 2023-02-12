// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerAnimatorManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player Animator Component controls.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using Photon.Pun;
using System;

namespace Photon.Pun.Demo.PunBasics
{
    public class PlayerAnimatorManager : MonoBehaviourPun
    {
        #region Private Fields

        [SerializeField]
        private float directionDampTime = 0.25f;
        Animator animator;

        Vector3 hitPosition;
        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            animator = GetComponent<Animator>();
            hitPosition = transform.position;
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {

            // Prevent control is connected to Photon and represent the localPlayer
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }

            // failSafe is missing Animator component on GameObject
            if (!animator)
            {
                return;
            }

            // deal with Jumping
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // only allow jumping if we are running.
            /*if (stateInfo.IsName("Base Layer.Run"))
            {
                // When using trigger parameter
                if (Input.GetButtonDown("Fire2")) animator.SetTrigger("Jump"); 
            }

            // deal with movement
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            // prevent negative Speed.
            if( v < 0 )
            {
                v = 0;
            }

            // set the Animator Parameters
            animator.SetFloat( "Speed", h*h+v*v );
            animator.SetFloat( "Direction", h, directionDampTime, Time.deltaTime );*/

            if(PlayerManager.turnNum > PhotonNetwork.PlayerList.Length)
            {
                PlayerManager.turnNum = PhotonNetwork.PlayerList.Length;
                photonView.RPC("setTurnNum", RpcTarget.AllBufferedViaServer, PlayerManager.turnNum);
            }
            if (PhotonNetwork.PlayerList[PlayerManager.turnNum-1].ActorNumber == photonView.Owner.ActorNumber)
            {

                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit;
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {
                        hitPosition = hit.point;
                        float angle = Vector3.Angle(transform.forward, hit.point - transform.position);
                        Vector3 cross = Vector3.Cross(transform.forward, hit.point - transform.position);
                        if (cross.y < 0) angle = -angle;
                        transform.Rotate(0, angle, 0);
                    }
                }

                if (Vector3.Distance(transform.position, hitPosition) > 2f && PlayerManager.canStart)
                {
                    animator.SetFloat("Speed", 1);
                }
                else if (PlayerManager.canStart)
                {
                    PlayerManager.canStart = false;
                    animator.SetFloat("Speed", 0);
                    PlayerManager.turnNum++;
                    if (PlayerManager.turnNum > PhotonNetwork.PlayerList.Length) PlayerManager.turnNum = 1;
                    photonView.RPC("setTurnNum", RpcTarget.AllBufferedViaServer, PlayerManager.turnNum);
                }
            }
        }

        [PunRPC]
        public void setTurnNum(int TurnNum)
        {
            PlayerManager.turnNum = TurnNum;
        }
        #endregion
    }
}