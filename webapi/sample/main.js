
function convert(d) {
  var formattedDate = function(date) {
        return date.getFullYear()
          + '/' + ('0' + (date.getMonth() + 1)).slice(-2)
          + '/' + ('0' + date.getDate()).slice(-2)
          + ' ' + ('0' + date.getHours()).slice(-2)
          + ':' + ('0' + date.getMinutes()).slice(-2)
          + ':' + ('0' + date.getSeconds()).slice(-2)
      };
  d.time = formattedDate(new Date(d.time))
  return d
}

function listed(data) {
  data.map(d => convert(d))
  return data
}

var app = new Vue({
  el: '#app',
  data: {
    url: "",
    mode: 1,
    period: '5m',
    periods: [
      { value: '5m', text: '5 munutes'},
      { value: '10m', text: '10 munutes'},
      { value: '1h', text: '1 hour'},
      { value: 'today', text: 'Today'},
    ],
    result: [],
    fields: [],
    user: null,
    location: null,
    description: null,

    users: [],
    locations: [],

    error: null,
  },
  mounted() {
    axios
      .get("./settings.json")
      .then(response => {
        this.url = response.data['url']
        axios
          .get(`${this.url}/user/`)
          .then(response => (this.users = response.data))
        axios
          .get(`${this.url}/location/`)
          .then(response => (this.locations = response.data))
      })
      .catch(error => this.error = error)
  },
  methods: {
    chenged_value: function() {
      this.search()
    },
    search: function() {
      switch(this.mode) {
        case 1: this.search_all(); break;
        case 2: this.search_user(); break;
        case 3: this.search_description(); break;
        case 4: this.search_location(); break;
      }
      this.fields = [
        { key: 'name', sortable: true },
        { key: 'location', sortable: true },
        { key: 'time', sortable: true },
      ]
    },
    search_all: function() {
      axios
        .get(`${this.url}/search/${this.period}`)
        .then(response => this.result = listed(response.data))
        .catch(error => {
          this.result = []
        })
    },
    search_user: function() {
      axios
        .get(`${this.url}/search/byuser/${this.user}?period=${this.period}`)
        .then(response => this.result = [convert(response.data)])
        .catch(error => {
          this.result = []
        })
    },
    search_description: function() {
      exist = axios.post(
          `${this.url}/search/byuserdescription?period=${this.period}`,
          { description: this.description })
      users = axios.post(
        `${this.url}/user/`,
        { description: this.description })

      Promise
        .all([exist, users])
        .then((result) => {
          x = result[1].data.map(user => {
            r = result[0].data.find(r => r.name == user.name)
            return (typeof(r) != "undefined")
              ? r
              : { name: user.name, location: null, time: null }
          })
          this.result = convert(x)
        })
        .catch(error => {
          this.result = []
        })
    },
    search_location: function() {
      axios
        .get(`${this.url}/search/bylocation/${this.location}?period=${this.period}`)
        .then(response => this.result = convert(response.data))
        .catch(error => {
          this.result = []
        })
    },
    select_all: function() {
      this.mode = 1
      this.search()
    },
    select_user: function() {
      this.mode = 2
      this.search()
    },
    select_descripton: function() {
      this.mode = 3
      this.search()
    },
    select_location: function() {
      this.mode = 4
      this.search()
    },
    show_users: function() {
      axios
        .get(`${this.url}/user/`)
        .then(response => {
          this.users = response.data
          this.fields = [
            { key: 'id', label: 'ID', sortable: true },
            { key: 'name', label: 'Name', sortable: true  },
            { key: 'description', label: 'Description', sortable: true }
          ]
          this.result = this.users
        })
        .catch(error => this.error = error)
    },
    show_locations: function() {
      axios
        .get(`${this.url}/location/`)
        .then(response => {
          this.locations = response.data
          this.fields = [
            { key: 'id', label: 'ID', sortable: true },
            { key: 'location', label: 'Location', sortable: true },
            { key: 'uuid', label: 'UUID', sortable: true },
            { key: 'major', label: 'Major', sortable: true },
            { key: 'minor', label: 'Minor', sortable: true },
          ]
          this.result = this.locations
        })
        .catch(error => this.error = error)
    },
  }
});
