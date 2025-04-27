using UnityEngine;

public class Block : MonoBehaviour
{
    private const int BLOCK_LAYER = 8;

    private void Awake()
    {
        if (!GetComponent<BoxCollider2D>())
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
        gameObject.layer = BLOCK_LAYER;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Vérifie si le joueur entre en collision
        {
            // Élever le joueur en fonction de la direction
            Vector3 elevation = new Vector3(0, 1f, 0); // Élévation par défaut
            Vector2 playerVelocity = other.GetComponent<Rigidbody2D>().linearVelocity; // Récupérer la vélocité du joueur

            // Ajuster l'élévation en fonction de la direction du mouvement
            if (playerVelocity.x > 0) // Si le joueur se déplace vers la droite
            {
                elevation = new Vector3(0.5f, 1f, 0); // Élever légèrement vers la droite
            }
            else if (playerVelocity.x < 0) // Si le joueur se déplace vers la gauche
            {
                elevation = new Vector3(-0.5f, 1f, 0); // Élever légèrement vers la gauche
            }

            other.transform.position += elevation; // Appliquer l'élévation
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Vérifie si le joueur sort de la collision
        {
            // Ramener le joueur à sa position d'origine
            Vector3 elevation = new Vector3(0, -1f, 0); // Ramener par défaut
            Vector2 playerVelocity = other.GetComponent<Rigidbody2D>().linearVelocity; // Récupérer la vélocité du joueur

            // Ajuster la descente en fonction de la direction du mouvement
            if (playerVelocity.x > 0) // Si le joueur se déplace vers la droite
            {
                elevation = new Vector3(-0.5f, -1f, 0); // Ramener légèrement vers la droite
            }
            else if (playerVelocity.x < 0) // Si le joueur se déplace vers la gauche
            {
                elevation = new Vector3(0.5f, -1f, 0); // Ramener légèrement vers la gauche
            }

            other.transform.position += elevation; // Appliquer la descente
        }
    }
}