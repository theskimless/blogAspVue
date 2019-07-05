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
        adminPageErrors: [],
        authErrors: [],

        searchInput: "",

        articles: [],
        selectedArticleKey: 0,

        rubrics: [],
        selectedRubric: 0,

        loginForm: { login: "", password: "" },
        registerForm: { login: "", email: "", password: "", confirmPassword: "" },
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
            this.adminPageErrors.length = [];
            makeRequest("post", "blog/createRubric", this.addRubricForm)
                .then(response => {
                    if (response.status === 200) window.location.reload();
                }).catch(err => _(err));
        },
        //ADD ARTICLE
        addArticle() {
            this.adminPageErrors.length = [];
            var addArticleForm = JSON.parse(JSON.stringify(this.addArticleForm));
            addArticleForm.rubrics = this.addArticleForm.rubrics.join(",");

            makeRequest("post", "blog/createArticle", addArticleForm)
                .then(response => {
                    if (response.status === 200) window.location.reload();
                }).catch(err => _(err));
        },
        //DELETE ARTICLE
        deleteArticle(articleId) {
            makeRequest("post", "blog/deleteArticle", { Id: articleId })
                .then(response => {
                    if (response.status === 200) window.location.reload();
                }).catch(err => _(err));
        },
        //DELETE RUBRIC
        deleteRubric(rubricKey) {
            makeRequest("post", "blog/deleteRubric", { Id: this.rubrics[rubricKey].id })
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
                if (this.articleRubrics[i].articleId === articleId && this.articleRubrics[i].rubricId === this.rubrics[this.selectedRubric].id) {
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

                    for (var rubricKey in this.rubrics) {
                        if (this.rubrics[rubricKey].id === this.articleRubrics[i].rubricId) {
                            rubric += this.rubrics[rubricKey].name;
                            counter++;
                            rubrics.push(rubric);

                            break;
                        }
                    }
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
                        rubrics.unshift({ id: 0, name: "Все" });
                    }
                    if (articles.length !== 0) {
                        //TRANSFORM DATE 1 Января, 2019
                        for (var key in articles) {
                            var newDate = new Date(articles[key].date);
                            articles[key].date = newDate.getDate() + " " + months[newDate.getMonth()] + ", " + newDate.getFullYear();
                            articles[key].isShowed = 1;
                        }

                        //SHOW THE NEWEST POSTS IN THE TOP
                        articles = articles.reverse();
                    }

                    this.articleRubrics = data.articleRubrics;
                    this.rubrics = rubrics;
                    this.articles = articles;
                }).catch(err => _(err));
        },
        //AUTH
        register() {
            axios.post("/register", this.registerForm)
                .then(response => {
                    if (response.status === 200) {
                        var data = response.data;
                        localStorage.setItem("token", data.token);
                        localStorage.setItem("refreshToken", data.refreshToken);
                    }

                }).catch(err => _(err));
        },
        login() {
            axios.post("/login", this.loginForm)
                .then(response => {
                    if (response.status === 200) {
                        var data = response.data;
                        localStorage.setItem("token", data.token);
                        localStorage.setItem("refreshToken", data.refreshToken);
                        this.isAdmin = true;
                        this.toggleAuth(-1);
                    }
                }).catch(err => _(err));
        },
        check() {
            makeRequest("get", "/check")
                .then(response => _(response))
                .catch(err => _(err));
        },
        logOut() {
            localStorage.removeItem("token");
            localStorage.removeItem("refreshToken");
            this.isAdmin = false;
        }
    },
    created() {
        this.getData();
        var jwtToken = localStorage.getItem("token");
        if (jwtToken !== null) {
            if (getTokenExp(jwtToken) - Date.now() > 0) this.isAdmin = true;
            else refreshToken()
                .then(() => { this.isAdmin = true; })
                .catch(err => _(err));
        }
    }
});

//JWT TOKEN
function refreshToken() {
    return new Promise((resolve, reject) => {
        axios.post("/refresh", { oldToken: localStorage.getItem("token"), oldRefreshToken: localStorage.getItem("refreshToken") })
            .then(response => {
                var data = response.data;
                localStorage.setItem("token", data.token);
                localStorage.setItem("refreshToken", data.refreshToken);
                resolve();
            })
            .catch(err => reject(err));
    });
}

function requestWithJwt(method, url, data, jwtToken) {
    return axios.request({
        url: url,
        method: method,
        headers: { Authorization: "Bearer " + jwtToken },
        data: data
    });
}

function getTokenExp(jwtToken) {
    var header = JSON.parse(atob(jwtToken.split(".")[1]));
    return header.exp * 1000;
}

//MAKE REQUEST WITH JWT
function makeRequest(method, url, data) {
    //CLEAR Errors
    return new Promise((resolve, reject) => {
        var jwtToken = localStorage.getItem("token");
        if (jwtToken === undefined || localStorage.getItem("refreshToken") === undefined) {
            app.adminPageErrors.push("Это действие доступно только зарегестрированным пользователям");
            reject("Log in or Sigh up");
        }

        if (getTokenExp(jwtToken) - Date.now() > 0) {
            resolve(requestWithJwt(method, url, data, jwtToken));
        }
        //REFRESH TOKEN AND MAKE REQUEST AGAIN
        else {
            refreshToken()
                .then(() => { resolve(requestWithJwt(method, url, data, jwtToken)); })
                .catch(err => reject(err));
        }
    });
}

//AXIOS ERRORS
function pushErrors(errorArray, errors) {
    for (var key in errors) {
        for (var key2 in errors[key]) {
            app[errorArray].push(errors[key][key2]);
        }
    }
}

axios.interceptors.response.use(response => {
    return response;
}, error => {
    if (error.response.status === 400) {
        if (typeof error.response.data === "object") {
            var errors = error.response.data.errors;

            //SHOW ERRORS
            switch (error.response.config.url) {
                case "blog/createArticle":
                case "blog/createRubric": pushErrors("adminPageErrors", errors); break;
                case "/login": pushErrors("authErrors", errors); break;
            }
        }
        if (error.response.config.url === "blog/deleteRubric") {
            if (typeof error.response.data !== "object") {
                app.adminPageErrors.push(error.response.data);
            }
        }
    }
    else if (error.response.status === 401) {
        if (error.response.headers["token-expired"] === "true") {
            refreshToken
                .catch(err => _(err));
        }
    }
    return error;
});