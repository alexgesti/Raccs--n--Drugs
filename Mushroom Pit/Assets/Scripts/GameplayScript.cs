using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayScript : MonoBehaviour
{
    public GameObject racoon;
    public GameObject cocaine;

    public List<Color> ColorList;

    [HideInInspector]
    public List<RacoonBehaviour> racoonList;
    public List<CocaineBehaviour> cocaineList;
    public int posRacoonList;

    public bool cameraTransition = false;

    public int maxCocaineBags = 6;
    public float offsetCocaineSpawn = 2f;

    [HideInInspector]
    public bool cocaineCanSpawn = false;
    public connection conect;
    private Transform playableArea;
    private Transform mainCamera;
    private Transform gamePos;

    public void Reset()
    {
        if (racoonList != null)
            DeleteList();
        cocaineList = new List<CocaineBehaviour>();

        if (racoonList != null)
            racoonList.Clear();
        racoonList = new List<RacoonBehaviour>();
        posRacoonList = -1;
    }

    void Awake()
    {
        playableArea = transform.GetChild(0);
        mainCamera = GameObject.Find("Main Camera").transform;
        gamePos = GameObject.Find("CameraGamePosition").transform;                                         
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraTransition)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, gamePos.position, 2 * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, gamePos.rotation, 2 * Time.deltaTime);

            if (Vector3.Distance(mainCamera.transform.position, gamePos.position) < 0.15)
            {
                cameraTransition = false;
                cocaineCanSpawn = true;
            }
        }
        else if (posRacoonList == 0 && cocaineCanSpawn)
            SpawnCocaine();
    }

    private void FixedUpdate()
    {
        //Send Position Racoon
        if(racoonList.Count > 0)
            conect.SendClientData(4);
    }

    public void LaunchGame(int size = 0)
    {
        cameraTransition = true;
        GameObject.Find("UI").SetActive(false);

        Transform[] pos = GameObject.Find("RacoonSpawn").GetComponentsInChildren<Transform>();
        for (int i = 0; i < size; i++)
        {
            if (i > 4)
                break;

            GameObject racc = Instantiate(racoon, pos[i + 1].position, pos[i + 1].rotation);
            RacoonBehaviour raccScript = racc.GetComponent<RacoonBehaviour>();
            raccScript.ChangeState(1);
            raccScript.gameplayScript = this;

            racc.transform.GetChild(1).GetComponent<Renderer>().material.color = ColorList[i];
            raccScript.colors[0] = ColorList[i];

            if (posRacoonList == i)
                raccScript.owned = true;
            racoonList.Add(raccScript);
        }
    }

    public void UpdateRacoon(Vector3 position, Vector3 rotation, int posRacoon)
    {
        racoonList[posRacoon].transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
    }

    public void ChargeRacoon(int posRacoon)
    {
        racoonList[posRacoon].ChangeState(4);
    }

    public void SpawnCocaine()
    {
        for (int i = 0; i < maxCocaineBags; i++)
        {
            Vector3 bounds = playableArea.GetComponent<Renderer>().bounds.size;
            Vector3 randPosition = new Vector3(
                (playableArea.position.x - bounds.x / 2) + Random.Range(offsetCocaineSpawn, bounds.x - offsetCocaineSpawn),
                cocaine.transform.position.y,
                (playableArea.position.z - bounds.z / 2) + Random.Range(offsetCocaineSpawn, bounds.z - offsetCocaineSpawn));

            GameObject obj = Instantiate(cocaine, randPosition, cocaine.transform.rotation);
            CocaineBehaviour cocaScript = obj.GetComponent<CocaineBehaviour>();
            cocaScript.gameplayScript = this;
            cocaScript.isBuffed = i == 0 ? true : false;
            cocaineList.Add(cocaScript);
        }

        cocaineCanSpawn = false;
        conect.SendClientData(3);
    }

    public void UpdateCocaine(Vector3 position, int posRacoon, bool isBuffed = false)
    {
        GameObject obj = Instantiate(cocaine, position, cocaine.transform.rotation);
        CocaineBehaviour cocaScript = obj.GetComponent<CocaineBehaviour>();
        cocaScript.gameplayScript = this;
        cocaScript.isBuffed = isBuffed;
        cocaineList.Add(cocaScript);
    }

    public void UpdateList(CocaineBehaviour cocaScript)
    { cocaineList.Remove(cocaScript); }

    public void DeleteList()
    {
        foreach (CocaineBehaviour cocaScript in cocaineList)
            Destroy(cocaScript.gameObject);

        cocaineList.Clear();
    }
}
