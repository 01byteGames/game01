using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StarManager : MonoBehaviour
{
    public static StarManager instance;

    [SerializeField] private GameObject[] _stars;
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private float _frameDelay = 0f;

    private int _totalSpawned;  // Total enemies in the round
    private int _killed;  // Number of enemies killed
    private Vector3[] _originalScales;  // To store the original scales of the stars
    private LevelManager _levelManager;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        _levelManager = GetComponent<LevelManager>();
    }
    public void Initialize()
    {
        // Initialize the original scales array and store the original scales of the stars
        _originalScales = new Vector3[_stars.Length];
        for (int i = 0; i < _stars.Length; i++)
        {
            if (_stars[i] != null)
            {
                _originalScales[i] = _stars[i].transform.localScale;  // Store the original scale
                _stars[i].SetActive(false);  // Optionally hide the stars at the start
            }
        }
    }

    // Call this function when the round ends
    public void DisplayStars()
    {
        Initialize();

        _killed = WaveManager.instance.GetTotalKilled();
        _totalSpawned = WaveManager.instance.GetTotalZombies();

        // Calculate how many stars to display based on enemies killed
        int starCount = CalculateStars();

        // Start the coroutine to show the stars with animation
        StartCoroutine(AnimateStars(starCount));

        // Unlock next level
        if (starCount > 0 && _levelManager != null)
            _levelManager.UnlockLevel(SceneManager.GetActiveScene().buildIndex);
    }

    // Calculate the number of stars based on killed enemies
    private int CalculateStars()
    {
        if (_killed == _totalSpawned)
            return 3;  // If all enemies are killed, show 3 stars
        else if (_killed >= _totalSpawned * 0.75f)
            return 2;  // Show 2 stars for 75% or more
        else if (_killed >= _totalSpawned * 0.5f)
            return 1;  // Show 1 star for 50% or more
        else
            return 0;  // Show no stars if less than 50%
    }

    
    IEnumerator AnimateStars(int starCount)
    {
        // Coroutine to animate the stars appearing one by one
        for (int i = 0; i < starCount; i++)
        {
            if (_stars[i] != null && _originalScales[i] != null)  // Safeguard: Check if star and scale are not null
            {
                // Make the star visible
                _stars[i].SetActive(true);

                // Set the star's initial scale to zero for scaling animation
                _stars[i].transform.localScale = Vector3.zero;

                // Animate the star scaling from zero to its original scale
                float currentTime = 0f;
                while (currentTime < _animationDuration)
                {
                    currentTime += Time.deltaTime;
                    float progress = currentTime / _animationDuration;

                    // Interpolate from zero scale to the original scale
                    _stars[i].transform.localScale = Vector3.Lerp(Vector3.zero, _originalScales[i], progress);

                    yield return null;
                }

                // Ensure the final scale is the original scale
                _stars[i].transform.localScale = _originalScales[i];

                // Optional: Add a slight delay between each star animation
                yield return new WaitForSeconds(_frameDelay);  // Delay between stars (adjust as needed)
            }
        }
    }
}
