export interface QuestionOption {
  id: number;
  optionText: string;
  sortOrder: number;
}

export interface Question {
  id: number;
  questionText: string;
  sortOrder: number;
  options: QuestionOption[];
}
