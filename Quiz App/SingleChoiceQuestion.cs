using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quiz_App
{
    //SingleChoiceQuestion class
    public class SingleChoiceQuestion : Question
    {
        public int ScQuestionId { get; set; }
        public string ScAnswer {  get; set; }

        public SingleChoiceQuestion(int scQuestionId, int questionId, int quizId, string questionText, string scAnswer) : base(questionId, quizId, questionText, "SingleChoice")
        {
            ScQuestionId = scQuestionId;
            ScAnswer = scAnswer;
        }

    }
}
