// SurveyType
export interface SurveyTypeModel { surveyTypeId: number; surveyTypeCd: string; name: string; }
export interface SurveyTypeCreateModel { surveyTypeCd: string; name: string; }
export interface SurveyTypeUpdateModel { surveyTypeId: number; surveyTypeCd: string; name: string; }

// Survey
export interface SurveyModel { surveyId: number; surveyTypeId: number; name: string; dtIni: string; dtFin?: string; }
export interface SurveyCreateModel { surveyTypeId: number; name: string; dtIni: string; dtFin?: string; }
export interface SurveyUpdateModel { surveyId: number; surveyTypeId: number; name: string; dtIni: string; dtFin?: string; }

// SurveyQuestion
export interface SurveyQuestionModel { surveyQuestionId: number; surveyId: number; questionId: number; question?: string; }
export interface SurveyQuestionCreateModel { surveyId: number; questionId: number; }

// SurveySup
export interface SurveySupModel { surveySupId: number; supUserId: number; surveyId: number; name: string; }
export interface SurveySupCreateModel { supUserId: number; surveyId: number; name: string; }
export interface SurveySupUpdateModel { surveySupId: number; supUserId: number; surveyId: number; name: string; }

// Question
export interface QuestionModel {
  questionId: number; isComplement: boolean; rank: number; question: string;
  description?: string; answerTypeCd: string; group?: string; metric?: string;
  iconMetric?: string; isFirstSurvey?: boolean; isFinalSurvey?: boolean;
  isCompetenceLevel?: boolean; isFinishEarly?: boolean; isStandardMetric?: boolean;
  isSglYesNoType?: boolean; isFeedback?: boolean;
}
export interface QuestionCreateModel extends Omit<QuestionModel, 'questionId'> {}
export interface QuestionUpdateModel extends QuestionModel {}

// QuestionOption
export interface QuestionOptionModel {
  questionOptionId: number; questionId: number; complementQuestionId?: number;
  rank: number; optionCd: string; description?: string; openMsgBox?: boolean; needNotes?: boolean;
}
export interface QuestionOptionCreateModel {
  questionId: number; complementQuestionId?: number; rank: number;
  optionCd: string; description?: string; openMsgBox?: boolean; needNotes?: boolean;
}
export interface QuestionOptionUpdateModel extends QuestionOptionModel {}
