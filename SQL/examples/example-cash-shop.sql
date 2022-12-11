DROP TABLE IF EXISTS `cash_shop_item`;
CREATE TABLE `cash_shop_item` (
  `id` int unsigned NOT NULL AUTO_INCREMENT COMMENT 'shop_id',
  `uniq_id` int unsigned DEFAULT '0' COMMENT 'Unique ID',
  `cash_name` varchar(255) NOT NULL COMMENT 'Sale Item Name',
  `main_tab` tinyint unsigned DEFAULT '1' COMMENT 'Main Tab Page 1-6',
  `sub_tab` tinyint unsigned DEFAULT '1' COMMENT 'Sub Tab Page 1-7',
  `level_min` tinyint unsigned DEFAULT '0' COMMENT 'Minimum level to buy',
  `level_max` tinyint unsigned DEFAULT '0' COMMENT 'Maximum level to buy',
  `item_template_id` int unsigned DEFAULT '0' COMMENT 'Item Template Id',
  `is_sell` tinyint unsigned DEFAULT '0' COMMENT 'Is it for sale',
  `is_hidden` tinyint unsigned DEFAULT '0' COMMENT 'Hidden item',
  `limit_type` tinyint unsigned DEFAULT '0',
  `buy_count` smallint unsigned DEFAULT '0',
  `buy_type` tinyint unsigned DEFAULT '0',
  `buy_id` int unsigned DEFAULT '0',
  `start_date` datetime DEFAULT '0001-01-01 00:00:00' COMMENT 'Sell start date',
  `end_date` datetime DEFAULT '0001-01-01 00:00:00' COMMENT 'Sell end date',
  `type` tinyint unsigned DEFAULT '0' COMMENT 'Currency Type',
  `price` int unsigned DEFAULT '0' COMMENT 'Sell price',
  `remain` int unsigned DEFAULT '0' COMMENT 'Remaining stock',
  `bonus_type` int unsigned DEFAULT '0' COMMENT 'Bonus type',
  `bouns_count` int unsigned DEFAULT '0' COMMENT 'Bonus amount',
  `cmd_ui` tinyint unsigned DEFAULT '0' COMMENT 'Whether to restrict one person at a time',
  `item_count` int unsigned DEFAULT '1' COMMENT 'Number of bundles',
  `select_type` tinyint unsigned DEFAULT '0',
  `default_flag` tinyint unsigned DEFAULT '0',
  `event_type` tinyint unsigned DEFAULT '0' COMMENT 'Event type',
  `event_date` datetime DEFAULT '0001-01-01 00:00:00' COMMENT 'Event time',
  `dis_price` int unsigned DEFAULT '0' COMMENT 'Current selling price',
  PRIMARY KEY (`id`) USING BTREE
) ENGINE=InnoDB DEFAULT AUTO_INCREMENT=20100054 CHARSET=utf8 COMMENT='In-game cashshop listings';

--
-- Dumping data for table `cash_shop_item`
--
LOCK TABLES `cash_shop_item` WRITE;
INSERT INTO `cash_shop_item` VALUES 
(20100011,20100011,'1-1',1,1,0,0,29176,0,0,0,0,0,0,'2019-05-01 14:10:08','2055-06-16 14:10:12',0,874,85,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100012,20100012,'1-2',1,2,0,0,29177,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100013,20100013,'1-3',1,3,0,0,29178,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100014,20100014,'1-4',1,4,0,0,29179,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100015,20100015,'1-5',1,5,0,0,29180,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100016,20100016,'1-6',1,6,0,0,29181,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100017,20100017,'1-7',1,7,0,0,29182,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100018,20100018,'2-1',2,1,0,0,29183,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100019,20100019,'2-1',2,1,0,0,29184,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100020,20100020,'2-2',2,2,0,0,29185,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100021,20100021,'2-3',2,3,0,0,29186,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100022,20100022,'2-4',2,4,0,0,29187,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100023,20100023,'2-5',2,5,0,0,29188,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100024,20100024,'2-6',2,6,0,0,29189,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100025,20100025,'2-7',2,7,0,0,29190,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100026,20100026,'3-1',3,1,0,0,29191,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100027,20100027,'3-2',3,2,0,0,29192,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100028,20100028,'3-3',3,3,0,0,29193,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100029,20100029,'3-4',3,4,0,0,29194,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100030,20100030,'3-5',3,5,0,0,29195,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100031,20100031,'3-6',3,6,0,0,29196,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100032,20100032,'3-7',3,7,0,0,29197,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100033,20100033,'4-1',4,1,0,0,29198,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100034,20100034,'4-2',4,2,0,0,29199,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100035,20100035,'4-3',4,3,0,0,29200,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100036,20100036,'4-4',4,4,0,0,29201,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100037,20100037,'4-6',4,5,0,0,29202,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100038,20100038,'4-6',4,6,0,0,29203,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100039,20100039,'4-7',4,7,0,0,29204,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100040,20100040,'5-1',5,1,0,0,29205,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100041,20100041,'5-2',5,2,0,0,29206,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100042,20100042,'5-3',5,3,0,0,29207,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100043,20100043,'5-4',5,4,0,0,29208,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100044,20100044,'5-5',5,5,0,0,29209,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100045,20100045,'5-6',5,6,0,0,29210,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100046,20100046,'5-7',5,7,0,0,29211,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100047,20100047,'6-1',6,1,0,0,29212,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100048,20100048,'6-2',6,2,0,0,29213,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100049,20100049,'6-3',6,3,0,0,29214,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100050,20100050,'6-4',6,4,0,0,29215,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100051,20100051,'6-5',6,5,0,0,29216,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100052,20100052,'6-6',6,6,0,0,29217,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0),
(20100053,20100053,'6-7',6,7,0,0,29218,0,0,0,0,0,0,'0001-01-01 00:00:00','0001-01-01 00:00:00',0,0,0,0,0,0,0,0,0,0,'0001-01-01 00:00:00',0)
;
UNLOCK TABLES;