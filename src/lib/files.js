//import
const fs = require("fs")
var global = require("./global")

//variables
const home = process.env.APPDATA || (process.platform == "darwin" ? process.env.HOME + "Library/Preferences" : process.env.HOME)
const path = home + "/.lanchat/"
var config = {}

module.exports = {

	//CONFIG//
	//load config
	configLoad: function () {
		if (load()) {
			return true
		}
	},

	//config write
	configWrite: function (type, value) {
		config[type] = value
		global[type] = value
		fs.writeFileSync(path + "config.json", JSON.stringify(config), function (err) {
			if (err) return console.log(err)
		})
	},

	//DATABASE//
	//load database
	loadDb: function () {
		db = JSON.parse(fs.readFileSync(path + "db.json", "utf8"))
		//export db
		global.db = db
	},

	//add user to db
	dbAddUser: function (nick) {
		global.db.push({
			nickname: nick,
		})

		//write to file
		fs.writeFileSync(path + "db.json", JSON.stringify(db), function (err) {
			if (err) return console.log(err)
		})
	},

	//write to db
	dbWrite: function (nick, key, value) {
		var index = global.db.findIndex(x => x.nickname === nick)
		global.db[index][key] = value
		fs.writeFileSync(path + "db.json", JSON.stringify(db), function (err) {
			if (err) return console.log(err)
		})
	}
}

//load config file
function load() {

	//check dir
	if (!fs.existsSync(home + "/.lanchat")) {
		fs.mkdirSync(home + "/.lanchat")
	}

	//check config
	if (!fs.existsSync(path + "config.json")) {
		// eslint-disable-next-line quotes
		fs.writeFileSync(path + "config.json", '{"nick":"default","port":"2137","notify":"mention", "devlog":false, "attemps": "5", "ratelimit": "15"}')
	}

	//check host database
	if (!fs.existsSync(path + "db.json")) {
		// eslint-disable-next-line quotes
		fs.writeFileSync(path + "db.json", '[]')
	}

	//load config
	try {

		//load file
		config = JSON.parse(fs.readFileSync(path + "config.json", "utf8"))

		//valdate
		if (!config.hasOwnProperty("nick")) {
			return false
		}
		if (!config.hasOwnProperty("port")) {
			return false
		}
		if (!config.hasOwnProperty("notify")) {
			return false
		}
		if (!config.hasOwnProperty("devlog")) {
			return false
		}
		if (!config.hasOwnProperty("ratelimit")) {
			return false
		}
		if (!config.hasOwnProperty("attemps")) {
			return false
		}

		//load motd
		if (fs.existsSync(home + "/.lanchat/motd.txt")) {
			config.motd = fs.readFileSync(home + "/.lanchat/motd.txt", "utf8")
		}

		//export config
		module.exports.config = config

		//return
		return true

	} catch (err) {

		//return
		return false
	}
}