using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;


public class ModelSelectCircle : MonoBehaviour
{
    #region TEMP - DELETE THIS ONCE MODELS ARE IN
        [SerializeField] private Material testMatDefault;
        [SerializeField] private Material testMat1;
        [SerializeField] private Material testMat2;
        [SerializeField] private Material testMat3;
    #endregion

    #region Circle Generation
        [SerializeField] private GameObject headPositionerPrefab;
        [SerializeField] private GameObject torsoPositionerPrefab;
        [SerializeField] private GameObject legsPositionerPrefab;

        private const float Z_POS_CURRENT_MODEL = -3.88f;   // Base radius

        private Dictionary<ModelID,GameObject> headDatabase = new Dictionary<ModelID,GameObject>();
        private Dictionary<ModelID,GameObject> torsoDatabase = new Dictionary<ModelID,GameObject>();
        private Dictionary<ModelID,GameObject> legsDatabase = new Dictionary<ModelID,GameObject>();

        private const float Z_POS_CURRENT_MODEL = -3.88f;   // Base radius
    #endregion

    [SerializeField] private GameObject headCircle;
    [SerializeField] private GameObject torsoCircle;
    [SerializeField] private GameObject legsCircle;

    private float ROTATION_ANGLE = 60;

    public ModelID currentHead {get; private set;}
    public ModelID currentTorso {get; private set;}
    public ModelID currentLegs {get; private set;}

    void Awake()
    {
        GameManager.Instance.LoadAllModels();
        headDatabase = GameManager.Instance.headDatabase;
        torsoDatabase = GameManager.Instance.torsoDatabase;
        legsDatabase = GameManager.Instance.legsDatabase;
    }

    void Start()
    {
        currentHead = 0;
        currentTorso = 0;
        currentLegs = 0;

        // Positioning everything
        int numModels = headDatabase.Count;
        ROTATION_ANGLE = 360f / numModels;

        float zValueMod = 0f;
        while(numModels > 4){
            zValueMod += Z_POS_CURRENT_MODEL / -4;
            numModels--;
        }

        // Move THIS game object away from the camera (center of the ModelSelectCircle) by zValueMod
        gameObject.transform.Translate( new Vector3(0,0,zValueMod), Space.Self );

        // Keep the current selected model's distance from the camera this way
        float radius = Z_POS_CURRENT_MODEL - zValueMod;     // Essentialy base radius + the mod

        // Generate the models in the right positions
        float rotationPositioner = 0;
        int tempColorPicker = 0;    // <- TEMP (DELETE ONCE MODLELS ARE IN)

        // Iterate through models
        foreach(ModelID modelId in System.Enum.GetValues(typeof(ModelID))) {
            if (modelId == ModelID.EndEnum)
            {
                break;
            }
            if (!headDatabase.ContainsKey(modelId))
            {
                continue;
            }

            GameObject newHeadPos = Instantiate(headPositionerPrefab, new Vector3(0,0,0), Quaternion.identity, headCircle.transform);
            newHeadPos.transform.Translate( new Vector3(0,0,headCircle.transform.position.z), Space.Self );     // Radius
            Transform defaultHeadTransform = headDatabase[modelId].transform;
            GameObject headModel = Instantiate(
                headDatabase[modelId],
                new Vector3(defaultHeadTransform.position.x, defaultHeadTransform.position.y, defaultHeadTransform.position.z - radius),
                defaultHeadTransform.rotation,
                newHeadPos.transform
            );
            // newHeadPos.GetComponentInChildren<MeshRenderer>().transform.Translate( new Vector3(0,0,radius) );   // TEMP?
            newHeadPos.transform.Rotate( new Vector3(0,rotationPositioner + 180f,0), Space.Self );                     // Rotation on the circle

            GameObject newTorsoPos = Instantiate(torsoPositionerPrefab, new Vector3(0,0,0), Quaternion.identity, torsoCircle.transform);
            newTorsoPos.transform.Translate( new Vector3(0,0,torsoCircle.transform.position.z), Space.Self );   // Radius
            Transform defaultTorsoTransform = torsoDatabase[modelId].transform;
            GameObject torsoModel = Instantiate(
                torsoDatabase[modelId],
                new Vector3(defaultTorsoTransform.position.x, defaultTorsoTransform.position.y, defaultTorsoTransform.position.z - radius),
                defaultTorsoTransform.rotation,
                newTorsoPos.transform
            );
            // newTorsoPos.GetComponentInChildren<MeshRenderer>().transform.Translate( new Vector3(0,0,radius) );  // TEMP?
            newTorsoPos.transform.Rotate( new Vector3(0,rotationPositioner + 180f,0), Space.Self );                    // Rotation on the circle

            GameObject newLegsPos = Instantiate(legsPositionerPrefab, new Vector3(0,0,0), Quaternion.identity, legsCircle.transform);
            newLegsPos.transform.Translate( new Vector3(0,0,legsCircle.transform.position.z), Space.Self );     // Radius
            Transform defaultLegsTransform = legsDatabase[modelId].transform;
            GameObject legsModel = Instantiate(
                legsDatabase[modelId],
                new Vector3(defaultLegsTransform.position.x, defaultLegsTransform.position.y, defaultLegsTransform.position.z - radius),
                defaultLegsTransform.rotation,
                newLegsPos.transform
            );
            // newLegsPos.GetComponentInChildren<MeshRenderer>().transform.Translate( new Vector3(0,0,radius) );   // TEMP?
            newLegsPos.transform.Rotate( new Vector3(0,rotationPositioner + 180f,0), Space.Self );                     // Rotation on the circle

            rotationPositioner += ROTATION_ANGLE;

            // TEMP SET COLOR - DELETE ALL OF THIS ONCE MODLELS ARE IN
            {
                Material tempMat = testMatDefault;
                switch(tempColorPicker){
                    case 1:
                        tempMat = testMat1;
                        break;
                    case 2:
                        tempMat = testMat2;
                        break;
                    case 3:
                        tempMat = testMat3;
                        break;
                }
                newHeadPos.GetComponentInChildren<MeshRenderer>().material = tempMat;
                newTorsoPos.GetComponentInChildren<MeshRenderer>().material = tempMat;
                newLegsPos.GetComponentInChildren<MeshRenderer>().material = tempMat;
                tempColorPicker++;
                if(tempColorPicker > 3){
                    tempColorPicker = 0;
                }
            }
        }
    }

    public void RotateHead(bool selectNext)
    {
        float positioner = GetRotationAngle(selectNext);
        headCircle.transform.Rotate( new Vector3(0,positioner,0), Space.Self );

        // Set the current part to the next or previous part
        do{
            currentHead = SetCurrentModel(selectNext,currentHead);
        }while(!headDatabase.ContainsKey(currentHead));
    }

    public void RotateTorso(bool selectNext)
    {
        float positioner = GetRotationAngle(selectNext);
        torsoCircle.transform.Rotate( new Vector3(0,positioner,0), Space.Self );

        // Set the current part to the next or previous part
        do{
            currentTorso = SetCurrentModel(selectNext,currentTorso);
        }while(!torsoDatabase.ContainsKey(currentTorso));
        
    }

    public void RotateLegs(bool selectNext)
    {
        float positioner = GetRotationAngle(selectNext);
        legsCircle.transform.Rotate( new Vector3(0,positioner,0), Space.Self );

        // Set the current part to the next or previous part
        do{
            currentLegs = SetCurrentModel(selectNext,currentLegs);
        }while(!legsDatabase.ContainsKey(currentLegs));
    }

    public GameObject[] GetCurrentParts()
    {
        Debug.Log(string.Format("Selected head: {0}, torso: {1}, legs: {2}",
            currentHead.ToString(), currentTorso.ToString(), currentLegs.ToString()));

        if(!headDatabase.ContainsKey(currentHead)){
            Debug.LogError("No head " + currentHead.ToString() + " found in database");
        }
        if(!torsoDatabase.ContainsKey(currentTorso)){
            Debug.LogError("No torso " + currentTorso.ToString() + " found in database");
        }
        if(!legsDatabase.ContainsKey(currentLegs)){
            Debug.LogError("No legs " + currentLegs.ToString() + " found in database");
        }

        GameObject[] ret = { headDatabase[currentHead], torsoDatabase[currentTorso], legsDatabase[currentLegs] };        
        return ret;
    }

    private ModelID SetCurrentModel(bool selectNext, ModelID current)
    {
        if(selectNext){
            current++;
            if(current == ModelID.EndEnum){
                current = 0;
            }
        }
        else{
            if(current == 0){
                current = (ModelID)((int)ModelID.EndEnum - 1);
            }
            else{
                current--;
            }            
        }
        return current;
    }

    private float GetRotationAngle(bool selectNext)
    {
        if(selectNext){
            return ROTATION_ANGLE * -1;
        }
        else{
            return ROTATION_ANGLE;
        }
    }
}
