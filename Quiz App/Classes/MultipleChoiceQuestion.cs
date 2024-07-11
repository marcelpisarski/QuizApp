using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz_App.Classes
{
    //MultipleChoiceQuestion class
    public class MultipleChoiceQuestion : Question
    {
        public int McQuestionId { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public int CorrectAnswerIndex { get; set; }

        public MultipleChoiceQuestion(int mcQuestionId, int questionId, int quizId, string questionText, string option1, string option2, string option3, string option4, int correctAnswerIndex)
        : base(questionId, quizId, questionText, "MultipleChoice")
        {
            McQuestionId = mcQuestionId;
            Option1 = option1;
            Option2 = option2;
            Option3 = option3;
            Option4 = option4;
            CorrectAnswerIndex = correctAnswerIndex;
        }

    }
}
