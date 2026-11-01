import { SubmitAnswer } from './submit-exam';

export interface ExamAnswerResult extends SubmitAnswer {
  isCorrect: boolean;
}

export interface ExamResult {
  id: number;
  fullName: string;
  score: number;
  totalQuestions: number;
  submittedAt: string;
  answers: ExamAnswerResult[];
}
