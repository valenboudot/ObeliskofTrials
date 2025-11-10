using UnityEngine;
using System;
using System.Collections;
using Photon.Pun;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider))]
public class ButtonSyncTrigger : MonoBehaviourPun
{
    [Header("Configuración de Interacción")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Configuración de Animación")]
    public string pressAnimationName = "ButtonPress";
    public string returnAnimationName = "ButtonReturn";
    public float pressedHoldTime = 0.5f;

    private Animator animator;

    private bool playerInRange = false;
    private bool isMoving = false;
    public event Action<PhotonMessageInfo> OnInteracted;

    private void Start()
    {
        animator = GetComponent<Animator>();
        GetComponent<BoxCollider>().isTrigger = true;
    }

    // --- (OnTriggerEnter y OnTriggerExit son idénticos, no cambian) ---
    #region Triggers de Proximidad
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.IsMine)
            {
                playerInRange = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView playerView = other.GetComponent<PhotonView>();
            if (playerView != null && playerView.IsMine)
            {
                playerInRange = false;
            }
        }
    }
    #endregion

    // --- (La lógica de red en Update y los RPCs es idéntica, no cambia) ---
    #region Lógica de Red
    private void Update()
    {
        if (playerInRange && !isMoving && Input.GetKeyDown(interactKey))
        {
            // Llama al Master
            photonView.RPC(nameof(Rpc_RequestPressButton), RpcTarget.MasterClient);

            // Predicción local (¡ahora llama a la nueva corrutina!)
            StartCoroutine(AnimateButtonCoroutine());
        }
    }

    [PunRPC]
    private void Rpc_RequestPressButton(PhotonMessageInfo info)
    {
        if (isMoving)
        {
            return;
        }
        OnInteracted?.Invoke(info);
        photonView.RPC(nameof(Rpc_AnimateButton), RpcTarget.All);
    }

    [PunRPC]
    private void Rpc_AnimateButton()
    {
        if (isMoving) return;
        StartCoroutine(AnimateButtonCoroutine());
    }
    #endregion

    // --- CORRUTINA ACTUALIZADA PARA ANIMACIÓN DE IDA Y VUELTA ---
    private IEnumerator AnimateButtonCoroutine()
    {
        isMoving = true; // El botón está ahora en una secuencia de animación

        if (animator != null)
        {
            // 1. Reproduce la animación de PRESIONAR
            animator.Play(pressAnimationName, 0, 0.0f);
        }
        else
        {
            Debug.LogError("¡Animator no encontrado en el botón!");
        }

        // 2. Espera el tiempo que el botón permanece presionado
        yield return new WaitForSeconds(pressedHoldTime);

        // 3. Reproduce la animación de VOLVER
        if (animator != null)
        {
            animator.Play(returnAnimationName, 0, 0.0f);
        }
        else
        {
            Debug.LogError("¡Animator no encontrado en el botón!");
        }

        // 4. Espera a que termine la animación de retorno
        // (Aquí asumimos que la animación de retorno dura lo mismo que el pressedHoldTime,
        // o puedes añadir una variable específica para la duración de la animación de retorno)
        // Para ser más precisos, puedes obtener la duración del clip:
        // AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        // yield return new WaitForSeconds(state.length);
        // Por ahora, usamos el mismo tiempo que la animación de "presionar" como placeholder.
        yield return new WaitForSeconds(pressedHoldTime); // <<< AJUSTA ESTO a la duración de tu animación de retorno

        isMoving = false; // El botón ha vuelto a su posición, listo para ser presionado de nuevo
    }
}