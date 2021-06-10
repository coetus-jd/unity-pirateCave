using System.Collections;
using System.Collections.Generic;
using PirateCave.Enums;
using UnityEngine;

namespace PirateCave.Controllers
{
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// Localiza o PhaseController
        /// </summary>
        private PhaseController phaseController;

        /// <summary>
        /// Quantidade de vida do jogador
        /// </summary>
        [Header("Life")]
        [SerializeField]
        private float life = 100;

        /// <summary>
        /// A velocidade com que o player irá se mover
        /// </summary>
        [Header("Move")]
        [SerializeField]
        private float velocity = 0.9f;

        private float horizontalMovement;

        /// <summary>
        /// Configurações
        /// </summary>
        [Header("Jump")]
        [SerializeField]
        private float jumpForce = 6.5f;

        /// <summary>
        /// Indica se o jogador está pulando
        /// </summary>
        private bool isJumping;

        /// <summary>
        /// Localizar o chão
        /// </summary>
        [Header("Ground")]
        [SerializeField]
        private LayerMask groundLayer;

        /// <summary>
        /// Guarda a referência para o objeto que representa o pé do player
        /// </summary>
        [SerializeField]
        private Transform Feet;

        /// <summary>
        /// Indica se o jogador está com os pés no chão
        /// </summary>
        private bool feetGround;

        /// <summary>
        /// O sprite do personagem
        /// </summary>
        private SpriteRenderer spriteRenderer;

        /// <summary>
        /// Animator com todas as animações do player
        /// </summary>
        private Animator animator;

        /// <summary>
        /// Indica se o jogador está correndo ou não
        /// </summary>
        private bool isRunning = false;

        private Rigidbody2D rigidBody;
        
        [Header("Lash")]
        /// <summary>
        /// Collider usado para dar danos nos personagens com a corrente
        /// </summary>
        [SerializeField]
        private BoxCollider2D slashCollider;
        
        /// <summary>
        /// Verifica se o player está atacando
        /// </summary>
        private bool isLashing;
        
        /// <summary>
        /// Guarda o último valor do movimento horizontal para sabermos se ele 
        /// parou de se mexer quando estava na esquerda ou direita
        /// </summary>
        private float lastHorizontalValue;

        void Start()
        {
            phaseController = GameObject.FindGameObjectWithTag(Tags.PhaseController)
                .GetComponent<PhaseController>();

            rigidBody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (life <= 0)
            {
                die();
                return;
            }

            feetGround = Physics2D.OverlapCircle(Feet.position, 0.1f, groundLayer);

            if (horizontalMovement != 0)
                lastHorizontalValue = horizontalMovement;

            movePlayer();
            lash();
        }

        // void OnBecameInvisible()
        // {
        //     die();
        // }
        
        void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.CompareTag(Tags.CorsairSlash))
                receiveDamage(10f);

            if (col.gameObject.CompareTag(Tags.SkeletonLash))
                receiveDamage(1f);
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.CompareTag(Tags.CorsairBullet))
            {
                Destroy(col.gameObject);
                receiveDamage(10f);
            }
        }

        public void receiveDamage(float damage)
        {
            life -= damage;
        }

        public void die()
        {
            phaseController?.youLosePanel?.SetActive(true);
            animator.SetFloat("walking", 0f);
            animator.SetBool("running", false);
            animator.SetBool("die", true);
            Destroy(gameObject, 1);
        }

        private void movePlayer()
        {
            horizontalMovement = Input.GetAxis("Horizontal");

            isRunning = Input.GetKey(KeyCode.LeftShift);

            handleMovement();
            handleAnimation();
            handlePlayerJump();
        }

        private void handlePlayerJump()
        {
            if (feetGround && Input.GetKeyDown(KeyCode.Space))
            {
                rigidBody.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
                isJumping = true;
            }
        }

        private void handleMovement()
        {
            float localVelocity = isRunning ? velocity * 2f : velocity;

            transform.Translate(((Vector3.right * localVelocity) * horizontalMovement) * Time.deltaTime);
        }
        
        private void handleAnimation()
        {
            // if (horizontalMovement == 0)
                // transform.Rotate(0, (lastHorizontalValue < 0f ? 180f : 0f), 0f);

            if (feetGround)
            {
                // spriteRenderer.flipX = (horizontalMovement < 0f);
                // transform.Rotate(0, (horizontalMovement >= 0f ? 0f : 180f), 0f);

                animator.SetFloat("walking", Mathf.Abs(horizontalMovement));
                animator.SetBool("running", isRunning);
            }
            else
            {
                animator.SetFloat("walking", 0f);
                animator.SetBool("running", false);
            }
            
            animator.SetBool("jump", isJumping);
        }

        /// <summary>
        /// Essa função será chamada ao terminar a animação de pulo
        /// </summary>
        private void stopJump() => isJumping = false;

        #region Lash attack

        private void lash()
        {
            if (Input.GetKeyDown(KeyCode.K) && !isLashing)
            {
                isLashing = true;
                animator.SetBool("lash", true);
            }
        }

        /// <summary>
        /// Essa função será chamada automaticamente pela animação quando
        /// a corrente estiver totalmente esticada
        /// </summary>
        private void enableLashCollider()
        {
            slashCollider.enabled = true;
        }

        /// <summary>
        /// Essa função será chamada automaticamente ao final da animação de lash
        /// </summary>
        private void disableLashCollider()
        {
            isLashing = false;
            slashCollider.enabled = false;
            stopTriggerAnimation("lash");
        }

        #endregion


        // <summary>
        /// Função auxiliar chamada por eventos nos finais das animações
        /// </summary>
        /// <param name="parameterName"></param>
        private void stopTriggerAnimation(string parameterName) => animator.SetBool(parameterName, false);
    }
}
