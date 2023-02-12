// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the networked player instance
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Photon.Pun.Demo.PunBasics
{
#pragma warning disable 649
    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    public class PlayerManager1 : MonoBehaviourPunCallbacks, IPunObservable
    {
        public static bool clicked = false;
        public static bool canStart = false;
        public static int totalNum = 0;
        public static Dictionary<string, bool> confirmList = new Dictionary<string, bool>();

        #region Public Fields

        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion

        #region Private Fields

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        private GameObject playerUiPrefab;

        //True, when the user is firing
        bool IsFiring;
        GameObject _uiGo;
        float prevTime;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        public void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
            }

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        public void Start()
        {
            // Create the UI
            if (this.playerUiPrefab != null)
            {
                _uiGo = Instantiate(this.playerUiPrefab);
                _uiGo.SendMessage("SetData", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><b>Missing</b></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

#if UNITY_5_4_OR_NEWER
            // Unity 5.4 has a new scene management. register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#endif
            prevTime = Time.time;
        }


        public override void OnDisable()
        {
            // Always call the base to remove callbacks
            base.OnDisable();

#if UNITY_5_4_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
#endif
        }

        private bool leavingRoom;

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// Process Inputs if local player.
        /// Show and hide the beams
        /// Watch for end of game, when local player health is 0.
        /// </summary>
        public void Update()
        {
            // we only process Inputs and check health if we are the local player
            if (Time.time > prevTime + 1)
            {
                updateUI();
                prevTime = Time.time;
                if (photonView.IsMine && clicked)
                {
                    photonView.RPC("updateList", RpcTarget.AllBufferedViaServer, photonView.Owner.UserId);
                    clicked = false;
                }
            }
        }

        public void updateUI()
        {
            if(_uiGo != null)
                _uiGo.SendMessage("SetData", this, SendMessageOptions.RequireReceiver);
        }

        [PunRPC]
        public void updateList(string userid)
        {
            if (confirmList.ContainsKey(photonView.Owner.UserId) == false)
            {
                confirmList[photonView.Owner.UserId] = true;
            }
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if(confirmList.ContainsKey(player.UserId) == false)
                {
                    return;
                }
            }
            confirmList.Clear();
            canStart = true;
        }

        public override void OnLeftRoom()
        {
            this.leavingRoom = false;
            //_uiGo.SendMessage("SetData", this, SendMessageOptions.RequireReceiver);
        }

#if !UNITY_5_4_OR_NEWER
        /// <summary>See CalledOnLevelWasLoaded. Outdated in Unity 5.4.</summary>
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }
#endif


        /// <summary>
        /// MonoBehaviour method called after a new level of index 'level' was loaded.
        /// We recreate the Player UI because it was destroy when we switched level.
        /// Also reposition the player if outside the current arena.
        /// </summary>
        /// <param name="level">Level index loaded</param>
        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }

            _uiGo = Instantiate(this.playerUiPrefab);
            _uiGo.SendMessage("SetData", this, SendMessageOptions.RequireReceiver);
        }

        #endregion

        #region Private Methods


#if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
#endif

        /// <summary>
        /// Processes the inputs. This MUST ONLY BE USED when the player has authority over this Networked GameObject (photonView.isMine == true)
        /// </summary>
        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting && photonView.Owner.IsMasterClient)
            {
                // We own this player: send the others our data
                //stream.SendNext(this.IsFiring);
                //stream.SendNext(this.Health);
            }
            else
            {
                // Network player, receive data
                //this.IsFiring = (bool)stream.ReceiveNext();
                //this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion
    }
}