const express = require('express');
const bodyParser = require('body-parser');
const cors = require('cors');
const fs = require('fs');

const app = express();
const timestamp=Date.now();
app.use(bodyParser.json());
app.use(cors());

app.use(express.static(__dirname));

dir = __dirname + '/pages';

app.get('/', (req, res) => {
    res.sendFile(dir + '/main.html');
});

app.get('/testGame', function(req, res) {
    res.sendFile(dir + '/index.html'); // send the html file in response to GET requests to '/'
});

app.post('/testGame', (req, res) => {
    const playerId = req.body.playerId;
    const data = { 
        "playerId": playerId,
        "timestamp": timestamp
 };

    fs.readFile('players.json', 'utf8', (err, jsonString) => {
        if (err) {
            console.log('读取文件出错:', err);
            return;
        }

        const players = JSON.parse(jsonString);
        players.push(data);

        fs.writeFile('players.json', JSON.stringify(players), (err) => {
            if (err) {
                console.log('写入文件出错:', err);
                return;
            }

            console.log('玩家已成功加入游戏:', playerId);
            res.sendStatus(200);
        });
    });
});

app.listen(3001, () => {
    console.log('服务器已启动，正在监听端口 3000');
});
