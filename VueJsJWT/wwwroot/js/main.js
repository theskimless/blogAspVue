function _(data) {
    console.log(data);
}

Vue.directive("click-outside", {
    bind(el, binding, vnode) {
        el.clickOutsideEvent = function (event) {
            if (!document.getElementsByClassName("w-top-user-cont")[0].contains(event.target) && !(event.target === el || el.contains(event.target))) {
                binding.value(-1);
            }
        };
        document.addEventListener("click", el.clickOutsideEvent);
    },
    unbind(el, binding) {
        document.removeEventListener("click", el.clickOutsideEvent);
        el.clickOutsideEvent = null;
    }
});

const months = ["Января", "Февраля", "Марта", "Апреля", "Мая", "Июня", "Июля", "Августа", "Сентября", "Октября", "Ноября", "Декабря"];
var app = new Vue({
    el: "#blog",
    data: {
        testurl: "check",
        //0 - Admin, 1 - Articles, 2 - ArticlePage
        selectedPage: 1,
        mods: ["Статьи", "Админ. панель"],
        isAuthShowed: -1,
        isAdmin: false,
        selectedAdminSections: [],
        adminPageWarnings: [],

        searchInput: "",

        articles: [],
        selectedArticleKey: 0,

        rubrics: [],
        selectedRubric: 0,

        addRubricForm: { name: "" },
        addArticleForm: { title: "", data: "", rubrics: [1] },

        msnry: null
    },
    watch: {
        searchInput(title) {
            title = title.toLowerCase();
            for (var key in this.articles) {
                if (this.articles[key].title.toLowerCase().search(title) !== -1) {
                    this.articles[key].isShowed = 1;
                } else this.articles[key].isShowed = 0;
            }
        }
    },
    methods: {
        toggleAuth(t) {
            this.isAuthShowed = t;
        },
        selectAdminSection(id) {
            Vue.set(this.selectedAdminSections, id, !this.selectedAdminSections[id]);
        },
        filetest(ev) {
            var reader = new FileReader();
            var t = this;
            reader.onload = function (e) {
                t.testurl = e.target.result;
            };

            reader.readAsDataURL(ev.target.files[0]);
        },
        //OPEN ARTICLE
        openArticle(articleId) {
            axios.get("blog/getArticle?articleId=" + articleId)
                .then(response => {
                    for (var key in this.articles) {
                        if (this.articles[key].id === articleId) {
                            if (response.data !== "") {
                                this.articles[key].data += response.data;
                            }
                            this.selectedArticleKey = key;
                            break;
                        }
                    }
                }).catch(err => _(err));
            this.selectedPage = 2;
        },
        //ADD RUBRIC
        addRubric() {
            axios.post("blog/createRubric", this.addRubricForm)
                .then(response => {
                    if (response.status === 200) window.location.reload();
                }).catch(err => _(err));
        },
        //ADD ARTICLE
        addArticle() {
            var addArticleForm = JSON.parse(JSON.stringify(this.addArticleForm));
            addArticleForm.rubrics = this.addArticleForm.rubrics.join(",");

            axios.post("blog/createArticle", addArticleForm)
                .then(response => {
                    if (response.status === 200) window.location.reload();
                }).catch(err => _(err));
        },
        //DELETE ARTICLE
        deleteArticle(articleId) {
            axios.post("blog/deleteArticle", { Id: articleId })
                .then(response => {
                    if (response.status === 200) window.location.reload();
                }).catch(err => _(err));
        },
        //DELETE RUBRIC
        deleteRubric(rubricId) {
            axios.post("blog/deleteRubric", { Id: rubricId })
                .then(response => {
                    if (response.status === 200) window.location.reload();
                }).catch(err => _(err));
        },
        appendRubricSelect() {
            for (var key = 1; key < this.rubrics.length; key++) {
                var stop = false;
                for (var i = 0; i < this.addArticleForm.rubrics.length; i++) {
                    if (this.addArticleForm.rubrics[i] === key) {
                        stop = true;
                        break;
                    }
                }
                if (!stop) {
                    this.addArticleForm.rubrics.push(key);
                    break;
                }
            }
        },
        removeRubricSelect(key) {
            if (key !== 0) this.addArticleForm.rubrics.splice(key, 1);
        },
        rubricIsNotSelected(key, rubricKey) {
            for (var rubric in this.addArticleForm.rubrics) {
                if (rubric != key && this.addArticleForm.rubrics[rubric] === rubricKey) return false;
            }
            return true;
        },
        //ARTICLES
        hasSelectedRubric(articleId) {
            //SHOW ALL POSTS
            if (this.selectedRubric === 0) return true;
            //SHOW IF HAS SELECTED RUBRIC
            for (var i = 0; i < this.articleRubrics.length; i++) {
                if (this.articleRubrics[i].articleId === articleId && this.articleRubrics[i].rubricId === this.selectedRubric) {
                    return true;
                }
            }
            return false;
        },
        getArticleRubrics(articleId) {
            var rubrics = [], counter = 0;
            for (var i = 0; i < this.articleRubrics.length; i++) {
                if (this.articleRubrics[i].articleId === articleId) {
                    var rubric = "";
                    if (counter !== 0) rubric += " / ";
                    rubric += this.rubrics[this.articleRubrics[i].rubricId];
                    counter++;
                    rubrics.push(rubric);
                }
            }
            return rubrics;
        },
        selectPage(id) {
            this.selectedPage = id;
        },
        getData() {
            axios.get("blog/getData")
                .then(response => {
                    var data = response.data;
                    var rubrics = data.rubrics;
                    var articles = data.articles;

                    if (rubrics.length !== 0) {
                        //data.Rubrics = [0: {Id: 1, Name: "Text"}]
                        rubrics = rubrics.map(p => p.name);
                        rubrics.unshift("Все");
                        //rubrics = [0: "Все", 1: "Text"]
                    }

                    if (articles.length !== 0) {
                        //TRANSFORM DATE 1 Января, 2019
                        for (var key in articles) {
                            var newDate = new Date(articles[key].date);
                            articles[key].date = newDate.getDate() + " " + months[newDate.getMonth()] + ", " + newDate.getFullYear();
                            articles[key].isShowed = 1;
                        }

                        //SHOW THE NEWEST POSTS IN THE TOP
                        articles = articles.slice().reverse();
                    }

                    this.articleRubrics = data.articleRubrics;
                    this.rubrics = rubrics;
                    this.articles = articles;
                }).catch(err => _(err));
        }
    },
    created() {
        this.getData();
    }
});

//AXIOS ERRORS
axios.interceptors.response.use(response => {
    return response;
}, error => {
    if (error.response.status === 400) {
        if (typeof error.response.data === "object") {
            for (var key in error.response.data.errors) {
                for (var key2 in error.response.data.errors[key]) {
                    app.adminPageWarnings.push(error.response.data.errors[key][key2]);
                }
            }
        }
        if (error.response.config.url === "blog/deleteRubric") {
            if (typeof error.response.data !== "object") {
                app.adminPageWarnings.push(error.response.data);
            }
        }
    }
    else if (error.response.status === 401 && error.response.headers["token-expired"] === "true") {
        var jwtToken = localStorage.getItem("token");

        axios.post("/refresh", { oldToken: localStorage.getItem("token"), oldRefreshToken: localStorage.getItem("refreshToken") })
            .then(response => {
                var data = response.data;

                localStorage.setItem("token", data.token);
                localStorage.setItem("refreshToken", data.refreshToken);
            }).catch(err => {
                _(err);
            });
    }
    return error;
});

var app2 = new Vue({
    el: "#app",
    data: {
        login: "",
        password: ""
    },
    methods: {
        reg() {
            axios.post("/register", { login: this.login, password: this.password })
                .then(response => {
                    _(response);
                    var data = response.data;

                    localStorage.setItem("token", data.token);
                    localStorage.setItem("refreshToken", data.refreshToken);
                }).catch(err => {
                    _(err);
                });
        },
        log() {

        },
        showToken() {
            var jwtToken = localStorage.getItem("token");
            _(jwtToken);
        },
        check() {
            var jwtToken = localStorage.getItem("token");
            axios.get("/check", { headers: { Authorization: "Bearer " + jwtToken} })
                .then(response => {
                    _(response);
                }).catch(err => {
                    _(err);
                });
        }
    }
});