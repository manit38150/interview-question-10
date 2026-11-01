export interface SubmitAnswer {
  questionId: number;
  selectedOptionId: number;
}

export interface SubmitExam {
  fullName: string;
  answers: SubmitAnswer[];
}
