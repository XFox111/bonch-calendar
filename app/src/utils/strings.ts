import LocalizedStrings from "react-localization";

const strings = new LocalizedStrings({
	en:
	{
		// MainView.tsx
		title_p1: "Bonch.{0}",
		title_p2: "Calendar",
		subtitle_p1: "Check your SPbSUT classes in {0} calendar",
		subtitle_p2: "your",
		pickFaculty: "1. Pick your faculty",
		pickCourse: "2. Pick your course",
		pickGroup: "3. Pick your group",
		pickGroup_empty: "No groups are available for the selected course",
		subscribe: "4. Subscribe to the calendar",
		copy: "Copy link",
		or: "or",
		download: "Download .ics file",
		cta: "Like the service? Tell your classmates!",

		// FaqView.tsx
		faq_h2: "Frequently asked questions",
		question1_h3: "How do I save timetable to my Outlook/Google calendar?",
		answer1_p1: "Once you picked your group, copy the generated link. Then, in your calendar app subscirbe to a new calendar using that link. Here're some guides:",
		answer1_li1: "Google Calendar",
		answer1_li2: "Outlook",
		answer1_li3: "Apple iCloud",
		answer1_p2: "Note that subscribing to a web calendar is available only on desktop versions of Google and Outlook. But once you subscribe, the timetable will be available on all your devices.",
		question2_h3: "The timetable can change. Do I need to re-import the calendar every time?",
		answer2_p1: "Unless you imported the calendar from file, no. Subscribed calendars update automatically. On our end, calendars update every six hours.",
		question3_h3: "My group/faculty doesn't appear in the list. How do I get my timetable?",
		answer3_p1: "If your faculty or group is missing, it probably means that the timetable for it is not yet published. Try again later.",
		answer3_p2: "If you already have a calendar link, it will work once the timetable is published.",
		question4_h3: "Do I need to re-import my calendar at the start of each semester/year?",
		answer4_p1: "No. The generated calendar link is valid indefinitely and will always point to the latest timetable for your group.",
		answer4_p2: "That being said, if your group or faculty changes their name, you might need to generate a new link, as the group and faculty IDs might change.",
		question5_h3: "Does the calendar show timetable from past semesters?",
		answer5_p1: "No. The calendar contains only the current semester's timetable. Once you enter a new semester, all past events will disappear.",
		answer5_p2: "If you want to keep past semesters' timetables, consider downloading them as files at the end of each semester.",
		question6_h3: "Something doesn't work (timetable doesn't show, the website is broken, etc.). How do I report this?",
		answer6_p1: "You can file an issue on project's {0} or contact me {1} (the former is preferred).",
		answer6_p1_link1: "GitHub page",
		answer6_p1_link2: "via email",
		answer6_p2: "Note that I am no longer a student and work on this project in my spare time. So if you want to get a fix quickly, consider submitting a pull request yourself. You can find all the necessary information on project's {0}.",
		answer6_p2_link: "GitHub page",
		question7_h3: "I want a propose a new feature. Do I file it on GitHub as well?",
		answer7_p1: "I do not accept any feature requests for this project. However, if you want to propose a new feature, then yes, you can still file an issue on project's {0} and maybe someone else will implement it.",
		answer7_p1_link: "GitHub page",
		answer7_p2: "The other way is to implement the feature yourself and submit a pull request. I do welcome contributions.",
		question8_h3: "GUT.Schedule app doesn't work anymore. Will it be fixed?",
		answer8_p1: "GUT.Schedule application is no longer supported. This project is a successor to that app, so please use it instead.",
		answer8_p2: "That being said, the GUT.Schedule app is open source as well, so if you'd like to tinker with it, you can find its source code {0}.",
		answer8_p2_link: "on GitHub",

		// DedicatedView.tsx
		dedicated_h2: "Dedicated to memory of Scientific and educational center \"Technologies of informational and educational systems\"",
		dedicated_p: "Always in our hearts ❤️",

		// FooterView.tsx
		footer_p1: "Made with ☕ and ❤️{0}by {1}",
		footer_p2: "Eugene Fox"
	},
	ru:
	{
		// MainView.tsx
		title_p1: "Бонч.{0}",
		title_p2: "Календарь",
		subtitle_p1: "Смотри расписание СПбГУТ в {0} календаре",
		subtitle_p2: "своем",
		pickFaculty: "1. Выбери свой факультет",
		pickCourse: "2. Выбери свой курс",
		pickGroup: "3. Выбери свою группу",
		pickGroup_empty: "Нет доступных групп для выбранного курса",
		subscribe: "4. Подпишись на календарь",
		copy: "Скопировать ссылку",
		or: "или",
		download: "Скачай .ics файл",
		cta: "Понравился сервис? Расскажи одногруппникам!",

		// FaqView.tsx
		faq_h2: "Часто задаваемые вопросы",
		question1_h3: "Как сохранить расписание в Outlook/Google календарь?",
		answer1_p1: "После того, как вы выбрали свою группу, скопируйте сгенерированную ссылку. Затем в своем календаре подпишитесь на новый календарь, используя эту ссылку. Вот несколько инструкций:",
		answer1_li1: "Google Календарь",
		answer1_li2: "Outlook",
		answer1_li3: "Apple Календарь",
		answer1_p2: "Обратите внимание, что в Google и Outlook подписаться на веб-календарь можно только в веб-версиях этих сервисов. Но после этого расписание будет доступно на всех ваших устройствах.",
		question2_h3: "Расписание может меняться. Нужно ли мне импортировать календарь каждый раз?",
		answer2_p1: "Если вы не импортировали календарь из файла, то нет. Календари на которые вы подписаны обновляются автоматически. С нашей стороны, календари обновляются каждые шесть часов.",
		question3_h3: "Моя группа/факультет не отображается в списке. Как мне получить свое расписание?",
		answer3_p1: "Если ваш факультет или группа отсутствует, скорее всего, расписание для них еще не опубликовано. Попробуйте позже.",
		answer3_p2: "Если у вас уже есть ссылка на календарь, можете использовать ее. Расписание появится в календаре сразу как только оно будет опубликовано.",
		question4_h3: "Нужно ли мне повторно импортировать календарь в начале каждого семестра/года?",
		answer4_p1: "Нет. Сгенерированная ссылка на календарь действительна бессрочно и всегда будет указывать на актуальное расписание для вашей группы.",
		answer4_p2: "Однако, если ваша группа или факультет изменили свое название, возможно, вам придется сгенерировать новую ссылку, так как идентификаторы группы или факультета могли также измениться.",
		question5_h3: "Показывает ли календарь расписание из прошлых семестров?",
		answer5_p1: "Нет. Календарь содержит только расписание текущего семестра. Как только начнется новый семестр, все прошедшие события исчезнут.",
		answer5_p2: "Если вы все же хотите сохранить расписания прошлых семестров, вы можете скачивать их в виде файлов в конце каждого семестра.",
		question6_h3: "Что-то не работает (расписание не отображается, сайт сломан и т.д.). Как мне об этом сообщить?",
		answer6_p1: "Вы можете создать задачу на {0} проекта или связаться со мной {1} (первый вариант предпочтительнее).",
		answer6_p1_link1: "странице GitHub",
		answer6_p1_link2: "по электронной почте",
		answer6_p2: "Обратите внимание, что я больше не являюсь студентом и работаю над этим проектом в свое свободное время. Поэтому, если вы хотите быстро получить исправление, вы можете самостоятельно создать пул реквест. Вся необходимая информация доступна на {0} проекта.",
		answer6_p2_link: "странице GitHub",
		question7_h3: "Я хочу предложить новую функцию. Это также делается через GitHub?",
		answer7_p1: "Я не принимаю запросы на добавление функций для этого проекта. Однако, если вы хотите предложить новую функцию, то да, вы можете создать задачу на {0} проекта, и, возможно, кто-то другой ее реализует.",
		answer7_p1_link: "странице GitHub",
		answer7_p2: "Другой способ - реализовать функцию самостоятельно и отправить пул реквест. Я приветствую сторонний вклад в проект.",
		question8_h3: "Приложение ГУТ.Расписание больше не работает. Его починят?",
		answer8_p1: "Приложение ГУТ.Расписание больше не поддерживается. Этот проект является его преемником.",
		answer8_p2: "Тем не менее, ГУТ.Расписание также имеет открытый исходный код, поэтому, если вы хотите с ним поэкспериментировать, вы можете найти его {0}.",
		answer8_p2_link: "на GitHub",

		// DedicatedView.tsx
		dedicated_h2: "Посвящается памяти научно-образовательного центра \"ТИОС\"",
		dedicated_p: "Навсегда в наших сердцах ❤️",

		// FooterView.tsx
		footer_p1: "Сделано с ☕ и ❤️,{0}{1}",
		footer_p2: "Евгений Лис"
	}
});

export default strings;
