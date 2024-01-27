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

        [SerializeField] private List<GameObject> models;   // Delete this once models are in

        private const float Z_POS_CURRENT_MODEL = -3.88f;   // Base radius

        private Dictionary<ModelID,GameObject> headDatabase = new Dictionary<ModelID,GameObject>();
        private Dictionary<ModelID,GameObject> torsoDatabase = new Dictionary<ModelID,GameObject>();
        private Dictionary<ModelID,GameObject> legsDatabase = new Dictionary<ModelID,GameObject>();
    #endregion

    [SerializeField] private GameObject headCircle;
    [SerializeField] private GameObject torsoCircle;
    [SerializeField] private GameObject legsCircle;

    private float ROTATION_ANGLE = 60;

    public ModelID currentHead {get; private set;}
    public ModelID currentTorso {get; private set;}
    public ModelID currentLegs {get; private set;}

    void Start()
    {
        currentHead = 0;
        currentTorso = 0;
        currentLegs = 0;

        // int numModels = headDatabase.Count;  // TODO (uncomment this and delete the following)
        LoadAllModels();

        // Positioning everything
        int fillModels = models.Count - headDatabase.Count;
        int numModels = models.Count;
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

        // TODO: Iterate through model databases instead
        // for(int mID = 0; mID < (int)ModelID.EndEnum; mID++)
        foreach(ModelID modelId in System.Enum.GetValues(typeof(ModelID))) {    // TODO: Iterate through model databases instead

            if (!headDatabase.ContainsKey(modelId))
            {
                continue;
            }

            GameObject newHeadPos = Instantiate(headPositionerPrefab, new Vector3(0,0,0), Quaternion.identity, headCircle.transform);
            newHeadPos.transform.Translate( new Vector3(0,0,headCircle.transform.position.z), Space.Self );     // Radius
            Transform defaultHeadTransform = headDatabase[modelId].transform;
            GameObject headModel = Instantiate(
                headDatabase[modelId],
                new Vector3(defaultHeadTransform.position.x * -1, defaultHeadTransform.position.y, defaultHeadTransform.position.x * -1 + radius),
                defaultHeadTransform.rotation * Quaternion.Euler(0, 180f, 0),
                newHeadPos.transform
            );
            // newHeadPos.GetComponentInChildren<MeshRenderer>().transform.Translate( new Vector3(0,0,radius) );   // TEMP?
            newHeadPos.transform.Rotate( new Vector3(0,rotationPositioner,0), Space.Self );                     // Rotation on the circle

            GameObject newTorsoPos = Instantiate(torsoPositionerPrefab, new Vector3(0,0,0), Quaternion.identity, torsoCircle.transform);
            newTorsoPos.transform.Translate( new Vector3(0,0,torsoCircle.transform.position.z), Space.Self );   // Radius
            Transform defaultTorsoTransform = torsoDatabase[modelId].transform;
            GameObject torsoModel = Instantiate(
                torsoDatabase[modelId],
                new Vector3(defaultTorsoTransform.position.x * -1, defaultTorsoTransform.position.y, defaultTorsoTransform.position.x * -1 + radius),
                defaultTorsoTransform.rotation * Quaternion.Euler(0, 180f, 0),
                newTorsoPos.transform
            );
            // newTorsoPos.GetComponentInChildren<MeshRenderer>().transform.Translate( new Vector3(0,0,radius) );  // TEMP?
            newTorsoPos.transform.Rotate( new Vector3(0,rotationPositioner,0), Space.Self );                    // Rotation on the circle

            GameObject newLegsPos = Instantiate(legsPositionerPrefab, new Vector3(0,0,0), Quaternion.identity, legsCircle.transform);
            newLegsPos.transform.Translate( new Vector3(0,0,legsCircle.transform.position.z), Space.Self );     // Radius
            Transform defaultLegsTransform = legsDatabase[modelId].transform;
            GameObject legsModel = Instantiate(
                legsDatabase[modelId],
                new Vector3(defaultLegsTransform.position.x * -1, defaultLegsTransform.position.y, defaultLegsTransform.position.x * -1 + radius),
                defaultLegsTransform.rotation * Quaternion.Euler(0, 180f, 0),
                newLegsPos.transform
            );
            // newLegsPos.GetComponentInChildren<MeshRenderer>().transform.Translate( new Vector3(0,0,radius) );   // TEMP?
            newLegsPos.transform.Rotate( new Vector3(0,rotationPositioner,0), Space.Self );                     // Rotation on the circle

            
            // TODO: Set model data and stuff, store in data structures of some sort prob


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

    private void LoadAllModels()
    {
        // Load in the models of each type from the relevant folder in Resources

        // Heads

        Object[] headList = Resources.LoadAll("Models/Heads");   //, typeof(ModelPart));
        Debug.Log(string.Format("Loaded {0} heads", headList.Length));
        foreach(Object h in headList){
            GameObject head = (GameObject)h;

            ModelID modelID = head.GetComponent<Part>().model_id;     // TODO: Get the ID for this model
            
            if(headDatabase.ContainsKey( modelID )){
                    continue;
            }
            
            // Add the model to the dictionary
            headDatabase.Add( modelID, head );
        }

        // Torsos

        Object[] torsoList = Resources.LoadAll("Models/Torsos");   //, typeof(ModelPart));
        Debug.Log(string.Format("Loaded {0} torsos", torsoList.Length));
        foreach (Object t in torsoList){
            GameObject torso = (GameObject)t;   // Cast by ModelPart ScriptableObject type
            ModelID modelID = torso.GetComponent<Part>().model_id;     // TODO: Get the ID for this model
                                                                      // 
            if (torsoDatabase.ContainsKey( modelID )){
                    continue;
            }
            
            // Add the model to the dictionary
            torsoDatabase.Add( modelID, torso );
        }

        // Legs

        Object[] legsList = Resources.LoadAll("Models/Legs");   //, typeof(ModelPart));
        Debug.Log(string.Format("Loaded {0} legs", legsList.Length));
        foreach (Object l in legsList){
            GameObject legs = (GameObject)l;   // Cast by ModelPart ScriptableObject type
            ModelID modelID = legs.GetComponent<Part>().model_id;     // TODO: Get the ID for this model
                                                                      // 
            if (legsDatabase.ContainsKey( modelID )){
                    continue;
            }
            
            // Add the model to the dictionary
            legsDatabase.Add( modelID, legs );
        }

        // If the number of heads =/= torsos =/= legs, throw an error
        if( headDatabase.Count != torsoDatabase.Count || torsoDatabase.Count != legsDatabase.Count ){
            Debug.LogError("The number of model parts do not match!");
        }
    }

    public void RotateHead(bool selectNext)
    {
        float positioner = GetRotationAngle(selectNext);
        headCircle.transform.Rotate( new Vector3(0,positioner,0), Space.Self );

        // Set the current part to the next or previous part
        // Debug.Log(currentHead.ToString());
        currentHead = SetCurrentModel(selectNext,currentHead);
        // Debug.Log(currentHead.ToString());
    }

    public void RotateTorso(bool selectNext)
    {
        float positioner = GetRotationAngle(selectNext);
        torsoCircle.transform.Rotate( new Vector3(0,positioner,0), Space.Self );

        // Set the current part to the next or previous part
        currentTorso = SetCurrentModel(selectNext,currentTorso);
    }

    public void RotateLegs(bool selectNext)
    {
        float positioner = GetRotationAngle(selectNext);
        legsCircle.transform.Rotate( new Vector3(0,positioner,0), Space.Self );

        // Set the current part to the next or previous part
        currentLegs = SetCurrentModel(selectNext,currentLegs);
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
