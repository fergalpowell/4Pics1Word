using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BuildManager : MonoBehaviour {

    private int level = 0;
    private string correct_answer = "BILL";
    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private bool coroutineStarted = false;
    private string generated_letters;
    private Sprite image1;
    private Sprite image2;
    private Sprite image3;
    private Sprite image4;
    private List<GameObject> letter_array = new List<GameObject>();
    private List<GameObject> answer_array = new List<GameObject>();

    public Image clue1;
    public Image clue2;
    public Image clue3;
    public Image clue4;
    public GameObject hint1;
    public GameObject hint2;
    public GameObject letters;
    public GameObject answer;
    public GameObject completed;
    public GameObject autoChoose;
    public GameObject deleteHint;
    public GameObject levelText;

    void Start () {
        // Loading Letters GameObjects into List
        foreach (Transform child in letters.transform){
            letter_array.Add(child.transform.gameObject);
        }

        // Loading Answer GameObjects into List
        foreach (Transform child in answer.transform){
            answer_array.Add(child.transform.gameObject);
        }

        UpdateLevel();
    }
    
    void Update () {
        if(!string.IsNullOrEmpty(answer_array[0].GetComponentInChildren<Text>().text) &&    // Checking if answer has been
            !string.IsNullOrEmpty(answer_array[1].GetComponentInChildren<Text>().text) &&   // completely filled out
            !string.IsNullOrEmpty(answer_array[2].GetComponentInChildren<Text>().text) &&
            !string.IsNullOrEmpty(answer_array[3].GetComponentInChildren<Text>().text)) {
                if(CheckAnswer() && coroutineStarted == false){
                    StartCoroutine("Wait");
                }
                if(!CheckAnswer()){
                    foreach(GameObject answer in answer_array){                     // Setting answer text red to
                        answer.GetComponentInChildren<Text>().color = Color.red;    // Indicate incorrect answer
                    }
                }
        }
        else{
            foreach(GameObject answer in answer_array){
                answer.GetComponentInChildren<Text>().color = Color.white;
            }
        }
    }

    // Setting coroutine variable to true to stop coroutine beginning again in update.
    // Waiting 2 seconds and then calling updateLevel()
    private IEnumerator Wait(){
        coroutineStarted = true;
        yield return new WaitForSeconds(2);
        UpdateLevel();
    }

    private void UpdateLevel(){
        coroutineStarted = false;
        if(level < 3){
            level++;
            levelText.GetComponent<Text>().text = level.ToString();
        }
        else{GameComplete();}
        LoadClues(level);  
        UpdateCorrectAnswer(level);
        autoChoose.SetActive(true);
        deleteHint.SetActive(true);

        // Generating random selection letters and including correct answer letters
        generated_letters = correct_answer;
        while(generated_letters.Length < 12){
            generated_letters += alphabet[UnityEngine.Random.Range(0, alphabet.Length)].ToString();
        }
        string shuffled_letters = Shuffle(generated_letters); // Shuffling letter selection

        // Updating Letters Text components
        for(int i = 0; i < letter_array.Count; i++){
            letter_array[i].GetComponentInChildren<Text>().text = shuffled_letters[i].ToString();
            letter_array[i].SetActive(true);
        }

        // Empty answer options
        for(int i = 0; i< answer_array.Count; i++){
            answer_array[i].GetComponentInChildren<Text>().text = null;
            answer_array[i].GetComponent<Button>().interactable = true; 
        }
    }

    private void LoadClues(int level){
        image1 = Resources.Load<Sprite>(String.Format("Images/{0}/1", level));
        clue1.GetComponent<Image>().sprite = image1;
        image2 = Resources.Load<Sprite>(String.Format("Images/{0}/2", level));
        clue2.GetComponent<Image>().sprite = image2;
        image3 = Resources.Load<Sprite>(String.Format("Images/{0}/3", level));
        clue3.GetComponent<Image>().sprite = image3;
        image4 = Resources.Load<Sprite>(String.Format("Images/{0}/4", level));
        clue4.GetComponent<Image>().sprite = image4;
    }

    private void UpdateCorrectAnswer(int level){
        switch (level){
            case 1:
                correct_answer = "BILL";
                break;
            case 2:
                correct_answer = "DATE";
                break;
            case 3:
                correct_answer = "SIGN";
                break;
            default:
                Debug.Log("Answer not updated");
                break;
        }
    }

    private string Shuffle(string str){
        char[] array = str.ToCharArray();
        return FisherYates(array);
    }

    // FisherYates algorithm used to shuffle letters
    private string FisherYates(char[] array)
    {
        int n = array.Length;
        System.Random rand = new System.Random();
        for (int i = 0; i < n; i++)
        {
            int r = i + rand.Next(n - i);
            char t = array[r];
            array[r] = array[i];
            array[i] = t;
        }
        return new string(array);
    }

    // Setting next empty answer option to recently clicked letter selection
    public void SelectionOnClick(){
        for(int i = 0; i < answer_array.Count; i++){
            if (string.IsNullOrEmpty(answer_array[i].GetComponentInChildren<Text>().text)){
                answer_array[i].GetComponentInChildren<Text>().text = 
                    EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text;
                EventSystem.current.currentSelectedGameObject.SetActive(false);
                break;
            }
        }
    }

    // Removing answer option when clicked and making letter selection reappear
    public void AnswerOnClick(){
        for(int i = 0; i < letter_array.Count; i++){
            if (letter_array[i].activeInHierarchy == false && letter_array[i].GetComponentInChildren<Text>().text == 
                    EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text){
                EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text = null;
                letter_array[i].SetActive(true);
                break;
            }
        }
    }

    // Comparing user answer to correct answer
    private bool CheckAnswer(){
        string user_answer = null;
        for(int i = 0; i< answer_array.Count; i++){
            user_answer += answer_array[i].GetComponentInChildren<Text>().text;
        }
        if(user_answer.Equals(correct_answer)){
            foreach(GameObject answer in answer_array){                             // Setting asnwer text green to 
                        answer.GetComponentInChildren<Text>().color = Color.green;  // indicate success
                    }   
            return true;
        }
        return false;
    }

    private void GameComplete(){
        completed.SetActive(true); // Displaying game completed notification
    }

    public void LoadScene(){
        SceneManager.LoadScene("main");
    }

    // Hint 1 - 1 correct letter will be transferred from the selection section to the answer section.
    public void AutoChoose(){
        int index = UnityEngine.Random.Range(0, 4);
        string random_letter = correct_answer[index].ToString();
        answer_array[index].GetComponentInChildren<Text>().text = random_letter;
        answer_array[index].GetComponent<Button>().interactable = false;
        foreach(GameObject letter in letter_array){
            if(letter.GetComponentInChildren<Text>().text == random_letter){
                letter.SetActive(false);
                break;
            }
        }
        EventSystem.current.currentSelectedGameObject.SetActive(false);
    }

    // Hint 2 - 1 incorrect letter will be removed from the selection section
    public void DeleteHint(){
        int index = UnityEngine.Random.Range(4, generated_letters.Length);
        string random_letter = generated_letters[index].ToString();
        foreach(GameObject letter in letter_array){
            if(letter.GetComponentInChildren<Text>().text == random_letter){
                letter.SetActive(false);
                break;
            }
        }
        EventSystem.current.currentSelectedGameObject.SetActive(false);
    }
}
