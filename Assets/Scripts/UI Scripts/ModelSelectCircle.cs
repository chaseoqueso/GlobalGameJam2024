using System.Collections;
using System.Collections.Generic;
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

        [SerializeField] private List<GameObject> models;

        private const float Z_POS_CURRENT_MODEL = -3.88f;   // Base radius

        // TODO: Update these to <modelID, ScriptableObject>
        private Dictionary<ModelID,GameObject> headDatabase = new Dictionary<ModelID,GameObject>();
        private Dictionary<ModelID,GameObject> torsoDatabase = new Dictionary<ModelID,GameObject>();
        private Dictionary<ModelID,GameObject> legsDatabase = new Dictionary<ModelID,GameObject>();
    #endregion

    [SerializeField] private GameObject headCircle;
    [SerializeField] private GameObject torsoCircle;
    [SerializeField] private GameObject legsCircle;

    private float ROTATION_ANGLE = 60;

    // public ModelPart currentHead;
    // public ModelPart currentTorso;
    // public ModelPart currentLegs;

    void Start()
    {
        // LoadAllModels();     // TODO

        // Positioning everything
        // int numModels = headDatabase.Count;  // TODO
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
        foreach(GameObject model in models){    // TODO: Iterate through model databases instead
            GameObject newHeadPos = Instantiate(headPositionerPrefab, new Vector3(0,0,0), Quaternion.identity, headCircle.transform);
            newHeadPos.transform.Translate( new Vector3(0,0,headCircle.transform.position.z), Space.Self );     // Radius
            newHeadPos.GetComponentInChildren<MeshRenderer>().transform.Translate( new Vector3(0,0,radius) );   // TEMP?
            newHeadPos.transform.Rotate( new Vector3(0,rotationPositioner,0), Space.Self );                     // Rotation on the circle

            GameObject newTorsoPos = Instantiate(torsoPositionerPrefab, new Vector3(0,0,0), Quaternion.identity, torsoCircle.transform);
            newTorsoPos.transform.Translate( new Vector3(0,0,torsoCircle.transform.position.z), Space.Self );   // Radius
            newTorsoPos.GetComponentInChildren<MeshRenderer>().transform.Translate( new Vector3(0,0,radius) );  // TEMP?
            newTorsoPos.transform.Rotate( new Vector3(0,rotationPositioner,0), Space.Self );                    // Rotation on the circle

            GameObject newLegsPos = Instantiate(legsPositionerPrefab, new Vector3(0,0,0), Quaternion.identity, legsCircle.transform);
            newLegsPos.transform.Translate( new Vector3(0,0,legsCircle.transform.position.z), Space.Self );     // Radius
            newLegsPos.GetComponentInChildren<MeshRenderer>().transform.Translate( new Vector3(0,0,radius) );   // TEMP?
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
        foreach(Object h in headList){
            GameObject head = (GameObject)h;   // Cast by ModelPart ScriptableObject type

            ModelID modelID = ModelID.ShovelKnight;     // TODO: Get the ID for this model
            
            if(headDatabase.ContainsKey( modelID )){
                    continue;
            }
            
            // Add the model to the dictionary
            headDatabase.Add( modelID, head );
        }

        // Torsos
        Object[] torsoList = Resources.LoadAll("Models/Torsos");   //, typeof(ModelPart));
        foreach(Object t in torsoList){
            GameObject torso = (GameObject)t;   // Cast by ModelPart ScriptableObject type

            ModelID modelID = ModelID.ShovelKnight;     // TODO: Get the ID for this model
            
            if(torsoDatabase.ContainsKey( modelID )){
                    continue;
            }
            
            // Add the model to the dictionary
            headDatabase.Add( modelID, torso );
        }

        // Legs
        Object[] legsList = Resources.LoadAll("Models/Legs");   //, typeof(ModelPart));
        foreach(Object l in legsList){
            GameObject legs = (GameObject)l;   // Cast by ModelPart ScriptableObject type

            ModelID modelID = ModelID.ShovelKnight;     // TODO: Get the ID for this model
            
            if(legsDatabase.ContainsKey( modelID )){
                    continue;
            }
            
            // Add the model to the dictionary
            headDatabase.Add( modelID, legs );
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

        // TODO: Set the current part to the next or previous part
    }

    public void RotateTorso(bool selectNext)
    {
        float positioner = GetRotationAngle(selectNext);
        torsoCircle.transform.Rotate( new Vector3(0,positioner,0), Space.Self );

        // TODO: Set the current part to the next or previous part
    }

    public void RotateLegs(bool selectNext)
    {
        float positioner = GetRotationAngle(selectNext);
        legsCircle.transform.Rotate( new Vector3(0,positioner,0), Space.Self );

        // TODO: Set the current part to the next or previous part
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
