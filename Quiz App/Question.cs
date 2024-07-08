using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz_App
{
    //Base question class
    public abstract class Question
    {
        public int QuestionId { get; set; }
        public int QuizId { get; set; }
        public string QuestionText {  get; set; }
        public string QuestionType { get; set; }

        protected Question(int questionId, int quizId, string questionText, string questionType) 
        {
            QuestionId = questionId;
            QuizId = quizId;
            QuestionText = questionText;
            QuestionType = questionType;
        }

    }
}
